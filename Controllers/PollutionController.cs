using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Controllers
{
    [Route("api/pollution")]
    [ApiController]
    [Authorize]
    public class PollutionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PollutionController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/pollution/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<EventPollutionDto>> GetEventPollution(int eventId)
        {
            var eventObj = await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                        .ThenInclude(o => o.ObjectMaterials)
                            .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(e => e.IdEvent == eventId);

            if (eventObj == null) return NotFound();

            var objectBreakdowns = new List<ObjectPollutionDto>();
            double eventSeverity = 0;

            foreach (var eventObject in eventObj.EventObjects)
            {
                var obj = eventObject.Object;
                if (obj.ObjectMaterials == null) continue;

                double kKatVal = (double)(eventObject.KKat ?? 1.0m);
                var materialBreakdowns = new List<MaterialPollutionDto>();
                double objectSeverity = 0;

                foreach (var objMaterial in obj.ObjectMaterials)
                {
                    var material = objMaterial.Material;
                    if (material?.BaseRate == null) continue;

                    double emitted = ResolveQuantity(objMaterial, obj);
                    if (emitted <= 0) continue;

                    double qN = Math.Max(emitted - (objMaterial.RecoveredQuantity ?? 0), 0);
                    if (qN <= 0) continue;

                    double contribution = qN * (double)material.BaseRate.Value * kKatVal;

                    materialBreakdowns.Add(new MaterialPollutionDto
                    {
                        MaterialId = material.IdMaterial,
                        MaterialName = material.Name,
                        QN = qN,
                        ToxicityFactor = material.ToxicityFactor,
                        KKat = eventObject.KKat ?? 1.0m,
                        SeverityContribution = contribution
                    });
                    objectSeverity += contribution;
                }

                if (materialBreakdowns.Count == 0) continue;

                objectBreakdowns.Add(new ObjectPollutionDto
                {
                    ObjectId = obj.IdObject,
                    ObjectName = obj.Name,
                    ComponentType = eventObject.ComponentType,
                    KKat = eventObject.KKat,
                    Materials = materialBreakdowns,
                    ObjectSeverity = objectSeverity
                });
                eventSeverity += objectSeverity;
            }

            return Ok(new EventPollutionDto
            {
                EventId = eventId,
                Objects = objectBreakdowns,
                TotalSeverityIndex = eventSeverity
            });
        }

        // GET api/pollution/events
        [HttpGet("events")]
        public async Task<ActionResult<List<object>>> GetAllEventsSeverity()
        {
            var events = await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                        .ThenInclude(o => o.ObjectMaterials)
                            .ThenInclude(om => om.Material)
                .ToListAsync();

            var results = events.Select(e =>
            {
                double severity = 0;
                foreach (var eo in e.EventObjects ?? [])
                {
                    var obj = eo.Object;
                    if (obj?.ObjectMaterials == null) continue;
                    double kKat = (double)(eo.KKat ?? 1.0m);
                    foreach (var om in obj.ObjectMaterials)
                    {
                        if (om.Material?.BaseRate == null) continue;
                        double emitted = ResolveQuantity(om, obj);
                        double qN = Math.Max(emitted - (om.RecoveredQuantity ?? 0), 0);
                        severity += qN * (double)om.Material.BaseRate.Value * kKat;
                    }
                }
                return new { e.IdEvent, e.EventType, e.EventDate, e.Location, TotalSeverityIndex = severity };
            })
            .OrderByDescending(x => x.TotalSeverityIndex)
            .ToList<object>();

            return Ok(results);
        }

        // Returns effective quantity in tonnes, matching the logic in DamageCalculationService.
        private static double ResolveQuantity(ObjectMaterial om, EnvironmentObject obj)
        {
            if (om.Mass.HasValue) return om.Mass.Value;
            if (om.Volume.HasValue) return om.Volume.Value;
            if (om.Percentage.HasValue)
            {
                double total = obj.TotalMass ?? obj.TotalVolume ?? 0;
                return om.Percentage.Value / 100.0 * total;
            }
            return 0;
        }
    }
}

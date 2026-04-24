using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Services
{
    public interface IDamageCalculationService
    {
        Task<decimal> CalculateDamageForEvent(int eventId);
        Task<EventDamageBreakdownDto> CalculateBreakdownForEvent(int eventId);
    }

    public class DamageCalculationService : IDamageCalculationService
    {
        private readonly AppDbContext _context;

        public DamageCalculationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns the total monetary damage (EUR) for an event using the Order No. 471 formula.</summary>
        public async Task<decimal> CalculateDamageForEvent(int eventId)
        {
            var breakdown = await CalculateBreakdownForEvent(eventId);
            return breakdown.TotalDamage;
        }

        /// <summary>
        /// Calculates a full per-material damage breakdown for an event.
        /// Formula per Order No. 471: Z_n = Q_n * T_n * K_kat * I_n
        /// where Q_n = emitted quantity, T_n = base rate, K_kat = category coefficient, I_n = indexing coefficient.
        /// </summary>
        public async Task<EventDamageBreakdownDto> CalculateBreakdownForEvent(int eventId)
        {
            var eventObj = await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                        .ThenInclude(o => o.ObjectMaterials)
                            .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(e => e.IdEvent == eventId);

            if (eventObj == null)
                return new EventDamageBreakdownDto { EventId = eventId, Objects = [], TotalDamage = 0 };

            var latestCoeff = await _context.IndexingCoefficients
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Quarter)
                .FirstOrDefaultAsync();
            decimal iN = latestCoeff?.Coefficient ?? 1.0m;

            var objectBreakdowns = new List<ObjectDamageBreakdownDto>();
            decimal eventTotal = 0;
            decimal eventPollutionTotal = 0;

            foreach (var eventObject in eventObj.EventObjects)
            {
                var obj = eventObject.Object;
                if (obj.ObjectMaterials == null) continue;

                string component = eventObject.ComponentType?.ToLowerInvariant() ?? "";
                decimal kKat = eventObject.KKat ?? 1.0m;

                var materialBreakdowns = new List<MaterialDamageBreakdownDto>();
                decimal objectTotal = 0;
                decimal objectPollutionTotal = 0;

                foreach (var objMaterial in obj.ObjectMaterials)
                {
                    var material = objMaterial.Material;
                    if (material?.BaseRate == null) continue;

                    double emitted = ResolveQuantity(objMaterial, obj);
                    if (emitted <= 0) continue;

                    double recovered = objMaterial.RecoveredQuantity ?? 0;
                    double qN = Math.Max(emitted - recovered, 0);
                    if (qN <= 0) continue;

                    decimal tN = material.BaseRate.Value;
                    string substanceType = material.SubstanceType?.ToLowerInvariant() ?? "standard";

                    decimal zN = ApplyFormula(tN, iN, qN, kKat, component, substanceType);

                    decimal pollutionSize = iN != 0 ? zN / iN : 0;

                    materialBreakdowns.Add(new MaterialDamageBreakdownDto
                    {
                        MaterialId = material.IdMaterial,
                        MaterialName = material.Name,
                        TN = tN,
                        IN = iN,
                        QN = (decimal)qN,
                        KKat = kKat,
                        ComponentType = eventObject.ComponentType,
                        SubstanceType = material.SubstanceType,
                        ZN = zN,
                        PollutionSize = pollutionSize
                    });
                    objectTotal += zN;
                    objectPollutionTotal += iN != 0 ? zN / iN : 0;
                }

                if (materialBreakdowns.Count == 0) continue;

                objectBreakdowns.Add(new ObjectDamageBreakdownDto
                {
                    ObjectId = obj.IdObject,
                    ObjectName = obj.Name,
                    ComponentType = eventObject.ComponentType,
                    KKat = eventObject.KKat,
                    Materials = materialBreakdowns,
                    ObjectDamage = objectTotal,
                    ObjectPollutionSize = objectPollutionTotal
                });
                eventTotal += objectTotal;
                eventPollutionTotal += objectPollutionTotal;
            }

            return new EventDamageBreakdownDto
            {
                EventId = eventId,
                IndexingCoefficient = iN,
                Objects = objectBreakdowns,
                TotalDamage = eventTotal,
                TotalPollutionSize = eventPollutionTotal
            };
        }

        private static decimal ApplyFormula(
            decimal tN, decimal iN, double qN, decimal kKat,
            string component, string substanceType)
        {
            if (component == "air")
                return tN * iN * (decimal)qN;

            bool isBds7OrSuspended = substanceType == "bds7" || substanceType == "suspended";
            if (isBds7OrSuspended && qN > 1.0)
            {
                decimal q08 = (decimal)Math.Pow(qN, 0.8);
                return tN * iN * q08 * kKat;
            }

            if (string.IsNullOrEmpty(component))
                return tN * iN * (decimal)qN * kKat;

            return tN * iN * (decimal)qN * kKat;
        }

        private static double ResolveQuantity(ObjectMaterial objMaterial, EnvironmentObject obj)
        {
            if (objMaterial.Mass.HasValue)
                return objMaterial.Mass.Value;
            if (objMaterial.Volume.HasValue)
                return objMaterial.Volume.Value;
            if (objMaterial.Percentage.HasValue)
            {
                double total = obj.TotalMass ?? obj.TotalVolume ?? 0;
                return (objMaterial.Percentage.Value / 100.0) * total;
            }
            return 0;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Models;
using ŽVPAIS_API.Services;

namespace ŽVPAIS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string[] allowedEventTypes = new[] { "gaisas", "medžiagų išsiliejimas", "stichija" };
        private readonly IDamageCalculationService _damageCalculationService;
        public EventsController(AppDbContext context, IDamageCalculationService damageCalculationService)
        {
            _context = context;
            _damageCalculationService = damageCalculationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                .ToListAsync();

            var writer = new GeoJsonWriter();
            var dtos = events.Select(e => new EventResponseDto
            {
                IdEvent = e.IdEvent,
                EventType = e.EventType,
                EventDate = e.EventDate,
                Description = e.Description,
                Location = e.Location,
                Polygon = writer.Write(e.Coordinates),
                Status = e.Status,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                Objects = e.EventObjects.Select(eo => new ObjectDto
                {
                    IdObject = eo.Object.IdObject,
                    Name = eo.Object.Name,
                    Description = eo.Object.Description
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetEvent(int id)
        {
            var @event = await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                .FirstOrDefaultAsync(e => e.IdEvent == id);

            if (@event == null)
                return NotFound();

            var writer = new GeoJsonWriter();
            var dto = new EventResponseDto
            {
                IdEvent = @event.IdEvent,
                EventType = @event.EventType,
                EventDate = @event.EventDate,
                Description = @event.Description,
                Location = @event.Location,
                Polygon = writer.Write(@event.Coordinates),
                Status = @event.Status,
                SensitivityFactor = @event.SensitivityFactor,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt,
                Objects = @event.EventObjects.Select(eo => new ObjectDto
                {
                    IdObject = eo.Object.IdObject,
                    Name = eo.Object.Name,
                    Description = eo.Object.Description
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> CreateEvent(EventCreateDto dto)
        {
            if (!allowedEventTypes.Contains(dto.EventType))
                return BadRequest($"Neteisingas tipas: {dto.EventType}");

            Polygon polygon;
            try
            {
                var reader = new GeoJsonReader();
                var geometry = reader.Read<Geometry>(dto.Polygon);
                polygon = geometry as Polygon;
                if (polygon == null)
                    return BadRequest("GeoJSON turi būti poligonas.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Neteisingas GeoJSON formatas: {ex.Message}");
            }

            var @event = new Event
            {
                EventType = dto.EventType,
                EventDate = dto.EventDate.ToUniversalTime(),
                Description = dto.Description,
                Location = dto.Location,
                Coordinates = polygon,
                Status = dto.Status,
                SensitivityFactor = dto.SensitivityFactor,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = null
            };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync(); // išsaugome, kad gautume Id

            // Pridedame objektus (jei yra)
            if (dto.ObjectIds != null && dto.ObjectIds.Any())
            {
                foreach (var objId in dto.ObjectIds)
                {
                    _context.EventObjects.Add(new EventObject
                    {
                        EventId = @event.IdEvent,
                        ObjectId = objId
                    });
                }
                await _context.SaveChangesAsync();
            }

            // Skaičiuojame žalą po to, kai objektai pridėti
            var damage = await _damageCalculationService.CalculateDamageForEvent(@event.IdEvent);
            var damageEvaluation = new DamageEvaluation
            {
                EventId = @event.IdEvent,
                Data = DateTime.UtcNow,
                ZalosDydis = (double)damage,      // konvertuojame į double
                PiniginisDydis = (double)damage,
                Notes = ""
            };
            _context.DamageEvaluations.Add(damageEvaluation);
            await _context.SaveChangesAsync();

            // Užkrauname sukurtą įvykį su objektais atsakymui
            var createdEvent = await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                .FirstOrDefaultAsync(e => e.IdEvent == @event.IdEvent);

            var writer = new GeoJsonWriter();
            var responseDto = new EventResponseDto
            {
                IdEvent = createdEvent.IdEvent,
                EventType = createdEvent.EventType,
                EventDate = createdEvent.EventDate,
                Description = createdEvent.Description,
                Location = createdEvent.Location,
                Polygon = writer.Write(createdEvent.Coordinates),
                Status = createdEvent.Status,
                CreatedAt = createdEvent.CreatedAt,
                UpdatedAt = createdEvent.UpdatedAt,
                Objects = createdEvent.EventObjects.Select(eo => new ObjectDto
                {
                    IdObject = eo.Object.IdObject,
                    Name = eo.Object.Name,
                    Description = eo.Object.Description
                }).ToList()
            };

            return CreatedAtAction(nameof(GetEvent), new { id = createdEvent.IdEvent }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, EventCreateDto dto)
        {
            var @event = await _context.Events
                .Include(e => e.EventObjects)
                .FirstOrDefaultAsync(e => e.IdEvent == id);

            if (@event == null)
                return NotFound();

            if (!allowedEventTypes.Contains(dto.EventType))
                return BadRequest($"Neteisingas tipas: {dto.EventType}");

            Polygon polygon;
            try
            {
                var reader = new GeoJsonReader();
                var geometry = reader.Read<Geometry>(dto.Polygon);
                polygon = geometry as Polygon;
                if (polygon == null)
                    return BadRequest("GeoJSON turi būti poligonas.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Neteisingas GeoJSON formatas: {ex.Message}");
            }

            @event.EventType = dto.EventType;
            @event.EventDate = dto.EventDate.ToUniversalTime();
            @event.Description = dto.Description;
            @event.Location = dto.Location;
            @event.Coordinates = polygon;
            @event.Status = dto.Status;
            @event.SensitivityFactor = dto.SensitivityFactor;
            @event.UpdatedAt = DateTimeOffset.UtcNow;

            // Atnaujiname objektų priskyrimus: ištriname senus, pridedame naujus
            _context.EventObjects.RemoveRange(@event.EventObjects);
            if (dto.ObjectIds != null && dto.ObjectIds.Any())
            {
                foreach (var objId in dto.ObjectIds)
                {
                    _context.EventObjects.Add(new EventObject
                    {
                        EventId = @event.IdEvent,
                        ObjectId = objId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
                return NotFound();

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("{id}/damage")]
        public async Task<ActionResult<decimal>> GetEventDamage(int id)
        {
            var damage = await _damageCalculationService.CalculateDamageForEvent(id);
            return Ok(damage);
        }
    }
}
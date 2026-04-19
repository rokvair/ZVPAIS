using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Services;

namespace ŽVPAIS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly AppDbContext _context;

        public EventsController(IEventService eventService, AppDbContext context)
        {
            _eventService = eventService;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventResponseDto>> GetEvent(int id)
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null) return NotFound();
            return Ok(@event);
        }

        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> CreateEvent(EventCreateDto dto)
        {
            try
            {
                var created = await _eventService.CreateEventAsync(dto);
                return CreatedAtAction(nameof(GetEvent), new { id = created.IdEvent }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> UpdateEvent(int id, EventCreateDto dto)
        {
            try
            {
                await _eventService.UpdateEventAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/damage")]
        public async Task<ActionResult<decimal>> GetEventDamage(int id)
        {
            var damage = await _eventService.GetEventDamageAsync(id);
            return Ok(damage);
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> ApproveEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.Status != "laukia peržiūros" && ev.Status != "tikrinamas")
                return BadRequest("Įvykis nėra laukiantis peržiūros.");
            ev.Status = "patvirtintas";
            ev.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> RejectEvent(int id, [FromBody] RejectDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.Status != "laukia peržiūros" && ev.Status != "tikrinamas")
                return BadRequest("Įvykis nėra laukiantis peržiūros.");
            ev.Status = "atmestas";
            ev.UpdatedAt = DateTimeOffset.UtcNow;
            // Optionally attach rejection note to the latest report
            if (!string.IsNullOrWhiteSpace(dto?.Notes))
            {
                var report = await _context.DamageEvaluations
                    .Where(r => r.EventId == id)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();
                if (report != null)
                {
                    report.Notes = (report.Notes ?? "") + $"\nAtmesta: {dto.Notes}";
                    _context.DamageEvaluations.Update(report);
                }
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class RejectDto
    {
        public string? Notes { get; set; }
    }
}

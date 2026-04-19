using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Repositories
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> GetAllAsync();
        Task<Event?> GetByIdAsync(int id);
        Task<Event?> GetByIdWithDetailsAsync(int id);
        Task<Event> AddAsync(Event @event);
        Task UpdateAsync(Event @event);
        Task DeleteAsync(int id);
        Task SetEventObjectsAsync(int eventId, IEnumerable<EventObjectAssignDto> eventObjects);
        Task AddDamageEvaluationAsync(DamageEvaluation evaluation);
    }

    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetAllAsync()
        {
            return await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                .ToListAsync();
        }

        public async Task<Event?> GetByIdAsync(int id)
        {
            return await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                .FirstOrDefaultAsync(e => e.IdEvent == id);
        }

        public async Task<Event?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                        .ThenInclude(o => o.ObjectMaterials)
                            .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(e => e.IdEvent == id);
        }

        public async Task<Event> AddAsync(Event @event)
        {
            @event.CreatedAt = DateTimeOffset.UtcNow;
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            return @event;
        }

        public async Task UpdateAsync(Event @event)
        {
            @event.UpdatedAt = DateTimeOffset.UtcNow;
            _context.Events.Update(@event);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetEventObjectsAsync(int eventId, IEnumerable<EventObjectAssignDto> eventObjects)
        {
            var existing = await _context.EventObjects
                .Where(eo => eo.EventId == eventId)
                .ToListAsync();
            _context.EventObjects.RemoveRange(existing);

            foreach (var dto in eventObjects)
            {
                _context.EventObjects.Add(new EventObject
                {
                    EventId = eventId,
                    ObjectId = dto.ObjectId,
                    ComponentType = dto.ComponentType,
                    KKat = dto.KKat
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddDamageEvaluationAsync(DamageEvaluation evaluation)
        {
            _context.DamageEvaluations.Add(evaluation);
            await _context.SaveChangesAsync();
        }
    }
}

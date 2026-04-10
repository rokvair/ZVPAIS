using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ŽVPAIS_API.Repositories;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Services
{
    public interface IEventService
    {
        Task<IEnumerable<EventResponseDto>> GetAllEventsAsync();
        Task<EventResponseDto> GetEventByIdAsync(int id);
        Task<EventResponseDto> CreateEventAsync(EventCreateDto dto);
        Task UpdateEventAsync(int id, EventCreateDto dto);
        Task DeleteEventAsync(int id);
    }

    public class EventService : IEventService
    {
        private readonly IEventRepository _repository;
        private readonly string[] allowedEventTypes = new[] { "gaisas", "medžiagų išsiliejimas", "stichija" };
        private readonly GeoJsonReader _geoJsonReader = new();
        private readonly GeoJsonWriter _geoJsonWriter = new();

        public EventService(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<EventResponseDto>> GetAllEventsAsync()
        {
            var events = await _repository.GetAllAsync();
            return events.Select(MapToDto);
        }

        public async Task<EventResponseDto> GetEventByIdAsync(int id)
        {
            var @event = await _repository.GetByIdAsync(id);
            return @event == null ? null : MapToDto(@event);
        }

        public async Task<EventResponseDto> CreateEventAsync(EventCreateDto dto)
        {
            if (!allowedEventTypes.Contains(dto.EventType))
                throw new ArgumentException($"Neteisingas įvykio tipas: {dto.EventType}. Galimi tipai: {string.Join(", ", allowedEventTypes)}");

            if (string.IsNullOrWhiteSpace(dto.Polygon))
                throw new ArgumentException("GeoJSON poligonas negali būti tuščias.");

            // Konvertuoti GeoJSON string į Polygon
            Polygon polygon;
            try
            {
                var geometry = _geoJsonReader.Read<Geometry>(dto.Polygon);
                polygon = geometry as Polygon ?? throw new ArgumentException("Pateiktas GeoJSON nėra poligonas.");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Neteisingas GeoJSON formatas: {ex.Message}");
            }

            var @event = new Event
            {
                EventType = dto.EventType,
                EventDate = dto.EventDate.ToUniversalTime(),
                Description = dto.Description,
                Location = dto.Location,
                Coordinates = polygon,
                Status = dto.Status,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var created = await _repository.AddAsync(@event);
            return MapToDto(created);
        }

        public async Task UpdateEventAsync(int id, EventCreateDto dto)
        {
            var @event = await _repository.GetByIdAsync(id);
            if (@event == null) throw new KeyNotFoundException("Event not found");

            if (!allowedEventTypes.Contains(dto.EventType))
                throw new ArgumentException($"Neteisingas įvykio tipas: {dto.EventType}. Galimi tipai: {string.Join(", ", allowedEventTypes)}");

            if (string.IsNullOrWhiteSpace(dto.Polygon))
                throw new ArgumentException("GeoJSON poligonas negali būti tuščias.");

            Polygon polygon;
            try
            {
                var geometry = _geoJsonReader.Read<Geometry>(dto.Polygon);
                polygon = geometry as Polygon ?? throw new ArgumentException("Pateiktas GeoJSON nėra poligonas.");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Neteisingas GeoJSON formatas: {ex.Message}");
            }

            @event.EventType = dto.EventType;
            @event.EventDate = dto.EventDate.ToUniversalTime();
            @event.Description = dto.Description;
            @event.Location = dto.Location;
            @event.Coordinates = polygon;
            @event.Status = dto.Status;
            @event.UpdatedAt = DateTimeOffset.UtcNow;

            await _repository.UpdateAsync(@event);
        }

        public async Task DeleteEventAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        private EventResponseDto MapToDto(Event @event)
        {
            return new EventResponseDto
            {
                IdEvent = @event.IdEvent,
                EventType = @event.EventType,
                EventDate = @event.EventDate,
                Description = @event.Description,
                Location = @event.Location,
                Polygon = _geoJsonWriter.Write(@event.Coordinates),
                Status = @event.Status,
                CreatedAt = @event.CreatedAt,
                UpdatedAt = @event.UpdatedAt
            };
        }
    }
}
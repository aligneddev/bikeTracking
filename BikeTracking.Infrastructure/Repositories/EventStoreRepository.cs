
using BikeTracking.Domain.Events;
using BikeTracking.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace BikeTracking.Infrastructure.Repositories;

/// <summary>
/// Event store repository for persisting domain events.
/// Provides immutable audit trail for event sourcing.
/// </summary>
public class EventStoreRepository : IEventStoreRepository
{
    private readonly BikeTrackingContext _context;

    public EventStoreRepository(BikeTrackingContext context)
    {
        _context = context;
    }

    public async Task AppendEventAsync(
        DomainEvent @event,
        CancellationToken cancellationToken = default)
    {
        var eventRecord = new Event
        {
            EventId = @event.EventId,
            AggregateId = @event.AggregateId,
            AggregateType = @event.AggregateType,
            EventType = @event.EventType,
            EventData = System.Text.Json.JsonSerializer.Serialize(@event),
            Timestamp = @event.Timestamp,
            Version = @event.Version,
            UserId = @event.UserId
        };

        _context.Events.Add(eventRecord);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsByAggregateIdAsync(
        Guid aggregateId,
        CancellationToken cancellationToken = default)
    {
        var events = await _context.Events
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        return events.Select(e =>
            System.Text.Json.JsonSerializer.Deserialize<DomainEvent>(e.EventData)!);
    }

    public async Task<IEnumerable<DomainEvent>> GetEventsByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var events = await _context.Events
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

        return events.Select(e =>
            System.Text.Json.JsonSerializer.Deserialize<DomainEvent>(e.EventData)!);
    }
}

public interface IEventStoreRepository
{
    Task AppendEventAsync(DomainEvent @event, CancellationToken cancellationToken = default);
    Task<IEnumerable<DomainEvent>> GetEventsByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    Task<IEnumerable<DomainEvent>> GetEventsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}

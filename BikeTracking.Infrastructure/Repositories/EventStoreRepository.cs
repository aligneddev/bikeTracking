
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
        DomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        var eventRecord = new Event
        {
            EventId = domainEvent.EventId,
            AggregateId = domainEvent.AggregateId,
            AggregateType = domainEvent.AggregateType,
            EventType = domainEvent.EventType,
            EventData = System.Text.Json.JsonSerializer.Serialize(domainEvent),
            Timestamp = domainEvent.Timestamp,
            Version = domainEvent.Version,
            UserId = domainEvent.UserId
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
    public Task AppendEventAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
    public Task<IEnumerable<DomainEvent>> GetEventsByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    public Task<IEnumerable<DomainEvent>> GetEventsByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}

namespace BikeTracking.Domain.Events;

public class DataDeletionRequested : DomainEvent
{
    public string Scope { get; init; } = null!; // "older_than_3_months" or "full_account"
    public bool IdentityVerified { get; init; }
}

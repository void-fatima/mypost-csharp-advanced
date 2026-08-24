using MyPost.Domain.Common;

namespace MyPost.Domain.Shipments;

public sealed class TrackingEvent
{
    private TrackingEvent() { }

    internal TrackingEvent(Guid shipmentId, ShipmentStatus status, string description, DateTimeOffset occurredAtUtc, Guid? actorUserId, string? location)
    {
        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        Status = status;
        Description = Guard.Required(description, nameof(description), 300);
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
        ActorUserId = actorUserId;
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
    }

    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string? Location { get; private set; }
}

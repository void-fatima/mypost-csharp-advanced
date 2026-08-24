using MyPost.Domain.Common;

namespace MyPost.Domain.Shipments;

public sealed class CourierAssignment
{
    private CourierAssignment() { }

    internal CourierAssignment(Guid shipmentId, Guid courierUserId, DateTimeOffset assignedAtUtc, Guid assignedByUserId)
    {
        if (courierUserId == Guid.Empty || assignedByUserId == Guid.Empty)
        {
            throw new DomainException("Courier and assigning administrator are required.");
        }

        Id = Guid.NewGuid();
        ShipmentId = shipmentId;
        CourierUserId = courierUserId;
        AssignedAtUtc = assignedAtUtc.ToUniversalTime();
        AssignedByUserId = assignedByUserId;
    }

    public Guid Id { get; private set; }
    public Guid ShipmentId { get; private set; }
    public Guid CourierUserId { get; private set; }
    public DateTimeOffset AssignedAtUtc { get; private set; }
    public Guid AssignedByUserId { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }
    public bool IsActive => EndedAtUtc is null;

    internal void End(DateTimeOffset endedAtUtc) => EndedAtUtc ??= endedAtUtc.ToUniversalTime();
}

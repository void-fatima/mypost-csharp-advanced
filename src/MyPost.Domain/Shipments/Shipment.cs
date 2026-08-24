using MyPost.Domain.Common;

namespace MyPost.Domain.Shipments;

public sealed class Shipment
{
    private static readonly IReadOnlyDictionary<ShipmentStatus, ShipmentStatus[]> AllowedTransitions =
        new Dictionary<ShipmentStatus, ShipmentStatus[]>
        {
            [ShipmentStatus.Created] = [ShipmentStatus.AwaitingPickup, ShipmentStatus.Cancelled],
            [ShipmentStatus.AwaitingPickup] = [ShipmentStatus.Accepted, ShipmentStatus.Cancelled],
            [ShipmentStatus.Accepted] = [ShipmentStatus.InTransit, ShipmentStatus.ReturnInitiated],
            [ShipmentStatus.InTransit] = [ShipmentStatus.OutForDelivery, ShipmentStatus.ReturnInitiated],
            [ShipmentStatus.OutForDelivery] = [ShipmentStatus.Delivered, ShipmentStatus.DeliveryFailed],
            [ShipmentStatus.DeliveryFailed] = [ShipmentStatus.OutForDelivery, ShipmentStatus.ReturnInitiated],
            [ShipmentStatus.ReturnInitiated] = [ShipmentStatus.ReturningToSender],
            [ShipmentStatus.ReturningToSender] = [ShipmentStatus.ReturnedToSender],
            [ShipmentStatus.Delivered] = [],
            [ShipmentStatus.ReturnedToSender] = [],
            [ShipmentStatus.Cancelled] = []
        };

    private readonly List<TrackingEvent> _trackingEvents = [];
    private readonly List<CourierAssignment> _assignments = [];

    private Shipment() { }

    public Shipment(
        Guid senderUserId,
        string trackingCode,
        AddressSnapshot senderAddress,
        string recipientName,
        string recipientPhone,
        AddressSnapshot destinationAddress,
        ShipmentType type,
        decimal weightGrams,
        Dimensions? dimensions,
        ServiceLevel serviceLevel,
        decimal calculatedPrice,
        DateTimeOffset nowUtc,
        string? customerReference = null)
    {
        if (senderUserId == Guid.Empty)
        {
            throw new DomainException("A sender is required.");
        }

        if (weightGrams <= 0 || weightGrams > 50_000)
        {
            throw new DomainException("Shipment weight must be between 0 and 50,000 grams.");
        }

        if (type == ShipmentType.Parcel && dimensions is null)
        {
            throw new DomainException("Parcel dimensions are required.");
        }

        if (calculatedPrice < 0)
        {
            throw new DomainException("Calculated price cannot be negative.");
        }

        Id = Guid.NewGuid();
        SenderUserId = senderUserId;
        TrackingCode = Guard.Required(trackingCode, nameof(trackingCode), 32).ToUpperInvariant();
        SenderAddress = senderAddress ?? throw new DomainException("A sender address is required.");
        RecipientName = Guard.Required(recipientName, nameof(recipientName), 160);
        RecipientPhone = Guard.Required(recipientPhone, nameof(recipientPhone), 30);
        DestinationAddress = destinationAddress ?? throw new DomainException("A destination address is required.");
        Type = type;
        WeightGrams = weightGrams;
        Dimensions = dimensions;
        ServiceLevel = serviceLevel;
        CalculatedPrice = decimal.Round(calculatedPrice, 2, MidpointRounding.AwayFromZero);
        Status = ShipmentStatus.Created;
        CreatedAtUtc = nowUtc.ToUniversalTime();
        UpdatedAtUtc = CreatedAtUtc;
        CustomerReference = string.IsNullOrWhiteSpace(customerReference) ? null : customerReference.Trim();
        _trackingEvents.Add(new TrackingEvent(Id, Status, "Shipment created", CreatedAtUtc, senderUserId, null));
    }

    public Guid Id { get; private set; }
    public string TrackingCode { get; private set; } = string.Empty;
    public Guid SenderUserId { get; private set; }
    public AddressSnapshot SenderAddress { get; private set; } = null!;
    public string RecipientName { get; private set; } = string.Empty;
    public string RecipientPhone { get; private set; } = string.Empty;
    public AddressSnapshot DestinationAddress { get; private set; } = null!;
    public ShipmentType Type { get; private set; }
    public decimal WeightGrams { get; private set; }
    public Dimensions? Dimensions { get; private set; }
    public ServiceLevel ServiceLevel { get; private set; }
    public decimal CalculatedPrice { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Guid? CourierUserId { get; private set; }
    public DeliveryResult? DeliveryResult { get; private set; }
    public string? DeliveryNote { get; private set; }
    public string? CustomerReference { get; private set; }
    public uint Version { get; private set; }
    public IReadOnlyCollection<TrackingEvent> TrackingEvents => _trackingEvents.AsReadOnly();
    public IReadOnlyCollection<CourierAssignment> Assignments => _assignments.AsReadOnly();

    public bool TransitionTo(ShipmentStatus next, DateTimeOffset atUtc, Guid? actorUserId, string description, string? location = null)
    {
        if (next == Status)
        {
            return false;
        }

        if (!AllowedTransitions[Status].Contains(next))
        {
            throw new DomainException($"Shipment cannot transition from {Status} to {next}.");
        }

        Status = next;
        UpdatedAtUtc = atUtc.ToUniversalTime();
        Version++;
        _trackingEvents.Add(new TrackingEvent(Id, next, description, UpdatedAtUtc, actorUserId, location));
        return true;
    }

    public bool AssignCourier(Guid courierUserId, Guid assignedByUserId, DateTimeOffset atUtc)
    {
        if (CourierUserId == courierUserId && _assignments.LastOrDefault()?.IsActive == true)
        {
            return false;
        }

        if (Status is ShipmentStatus.Delivered or ShipmentStatus.ReturnedToSender or ShipmentStatus.Cancelled)
        {
            throw new DomainException("A terminal shipment cannot be assigned.");
        }

        _assignments.LastOrDefault(assignment => assignment.IsActive)?.End(atUtc);
        CourierUserId = courierUserId;
        _assignments.Add(new CourierAssignment(Id, courierUserId, atUtc, assignedByUserId));
        UpdatedAtUtc = atUtc.ToUniversalTime();
        Version++;
        return true;
    }

    public void RecordDelivery(DeliveryResult result, string? note, DateTimeOffset atUtc, Guid courierUserId)
    {
        if (CourierUserId != courierUserId)
        {
            throw new DomainException("Only the assigned courier can record delivery.");
        }

        DeliveryResult = result;
        DeliveryNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        var next = result == global::MyPost.Domain.Shipments.DeliveryResult.Delivered
            ? ShipmentStatus.Delivered
            : ShipmentStatus.DeliveryFailed;
        TransitionTo(next, atUtc, courierUserId,
            result == global::MyPost.Domain.Shipments.DeliveryResult.Delivered ? "Shipment delivered" : $"Delivery failed: {result}");
    }

    public bool InitiateReturn(DateTimeOffset atUtc, Guid actorUserId, string reason) =>
        TransitionTo(ShipmentStatus.ReturnInitiated, atUtc, actorUserId, $"Return initiated: {Guard.Required(reason, nameof(reason), 240)}");
}

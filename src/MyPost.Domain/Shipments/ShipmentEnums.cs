namespace MyPost.Domain.Shipments;

public enum ShipmentType
{
    Letter = 1,
    Parcel = 2
}

public enum ShipmentStatus
{
    Created = 1,
    AwaitingPickup = 2,
    Accepted = 3,
    InTransit = 4,
    OutForDelivery = 5,
    DeliveryFailed = 6,
    Delivered = 7,
    ReturnInitiated = 8,
    ReturningToSender = 9,
    ReturnedToSender = 10,
    Cancelled = 11
}

public enum ServiceLevel
{
    Economy = 1,
    Standard = 2,
    Express = 3
}

public enum DeliveryResult
{
    Delivered = 1,
    RecipientUnavailable = 2,
    AddressNotFound = 3,
    Refused = 4,
    Damaged = 5,
    Other = 6
}

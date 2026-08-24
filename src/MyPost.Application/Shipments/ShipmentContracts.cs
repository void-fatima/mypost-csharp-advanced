using MyPost.Application.Common;
using MyPost.Domain.Shipments;

namespace MyPost.Application.Shipments;

public sealed record DimensionsRequest(decimal LengthCm, decimal WidthCm, decimal HeightCm);
public sealed record DestinationAddressRequest(string Label, string Line1, string City, string Province, string PostalCode, string Country = "Iran");

public sealed record CreateShipmentRequest(
    Guid SenderAddressId,
    string RecipientName,
    string RecipientPhone,
    DestinationAddressRequest Destination,
    ShipmentType Type,
    decimal WeightGrams,
    DimensionsRequest? Dimensions,
    ServiceLevel ServiceLevel,
    string? CustomerReference);

public sealed record TrackingEventDto(ShipmentStatus Status, string Description, DateTimeOffset OccurredAtUtc, string? Location);

public sealed record ShipmentSummaryDto(
    Guid Id,
    string TrackingCode,
    string RecipientName,
    string DestinationCity,
    ShipmentType Type,
    ServiceLevel ServiceLevel,
    decimal CalculatedPrice,
    ShipmentStatus Status,
    DateTimeOffset CreatedAtUtc,
    Guid? CourierUserId);

public sealed record ShipmentDetailDto(
    Guid Id,
    string TrackingCode,
    Guid SenderUserId,
    string RecipientName,
    string RecipientPhone,
    AddressView SenderAddress,
    AddressView DestinationAddress,
    ShipmentType Type,
    decimal WeightGrams,
    DimensionsRequest? Dimensions,
    ServiceLevel ServiceLevel,
    decimal CalculatedPrice,
    ShipmentStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    Guid? CourierUserId,
    DeliveryResult? DeliveryResult,
    string? DeliveryNote,
    IReadOnlyList<TrackingEventDto> History);

public sealed record AddressView(string Label, string Line1, string City, string Province, string PostalCode, string Country);

public sealed record PublicTrackingDto(
    string TrackingCode,
    string Recipient,
    string Destination,
    ShipmentType Type,
    ServiceLevel ServiceLevel,
    ShipmentStatus Status,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<TrackingEventDto> History);

public sealed record ShipmentFilter(
    int Page = 1,
    int PageSize = 20,
    ShipmentStatus? Status = null,
    string? Search = null,
    Guid? SenderUserId = null,
    Guid? CourierUserId = null)
{
    public PageRequest PageRequest => new(Page, PageSize);
}

public sealed record AssignCourierRequest(Guid CourierUserId);
public sealed record TransitionShipmentRequest(ShipmentStatus Status, string Description, string? Location);
public sealed record RecordDeliveryRequest(DeliveryResult Result, string? Note);
public sealed record InitiateReturnRequest(string Reason);

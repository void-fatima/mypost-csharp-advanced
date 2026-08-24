using MyPost.Application.Abstractions;
using MyPost.Application.Addresses;
using MyPost.Application.Common;
using MyPost.Application.Users;
using MyPost.Domain.Common;
using MyPost.Domain.Shipments;
using MyPost.Domain.Users;

namespace MyPost.Application.Shipments;

public sealed class ShipmentService(
    IShipmentRepository shipments,
    IAddressRepository addresses,
    IUserDirectory users,
    ITrackingCodeGenerator trackingCodes,
    IShipmentPriceCalculator prices,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<ShipmentDetailDto> CreateAsync(Guid senderUserId, CreateShipmentRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        if (!string.IsNullOrWhiteSpace(request.CustomerReference))
        {
            var existing = await shipments.GetByCustomerReferenceAsync(senderUserId, request.CustomerReference.Trim(), cancellationToken);
            if (existing is not null) return MapDetail(existing);
        }

        var senderAddress = await addresses.GetAsync(request.SenderAddressId, cancellationToken)
            ?? throw new NotFoundException("Sender address not found.");
        if (senderAddress.OwnerUserId != senderUserId)
        {
            throw new ForbiddenException("The sender address does not belong to the current customer.");
        }

        Dimensions? dimensions = request.Dimensions is null
            ? null
            : new Dimensions(request.Dimensions.LengthCm, request.Dimensions.WidthCm, request.Dimensions.HeightCm);
        var destination = new AddressSnapshot(
            request.Destination.Label,
            request.Destination.Line1,
            request.Destination.City,
            request.Destination.Province,
            request.Destination.PostalCode,
            request.Destination.Country);
        var trackingCode = await UniqueTrackingCodeAsync(cancellationToken);
        var shipment = new Shipment(
            senderUserId,
            trackingCode,
            senderAddress.Snapshot(),
            request.RecipientName,
            request.RecipientPhone,
            destination,
            request.Type,
            request.WeightGrams,
            dimensions,
            request.ServiceLevel,
            prices.Calculate(request.Type, request.WeightGrams, dimensions, request.ServiceLevel),
            clock.UtcNow,
            request.CustomerReference);

        shipment.TransitionTo(ShipmentStatus.AwaitingPickup, clock.UtcNow, senderUserId, "Shipment is awaiting postal acceptance");
        await shipments.AddAsync(shipment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapDetail(shipment);
    }

    public async Task<PagedResult<ShipmentSummaryDto>> ListOwnedAsync(Guid userId, ShipmentFilter filter, CancellationToken cancellationToken = default) =>
        MapPage(await shipments.SearchAsync(filter with { SenderUserId = userId, CourierUserId = null }, cancellationToken));

    public async Task<PagedResult<ShipmentSummaryDto>> ListAssignedAsync(Guid courierUserId, ShipmentFilter filter, CancellationToken cancellationToken = default) =>
        MapPage(await shipments.SearchAsync(filter with { CourierUserId = courierUserId, SenderUserId = null }, cancellationToken));

    public async Task<PagedResult<ShipmentSummaryDto>> ListAllAsync(ShipmentFilter filter, CancellationToken cancellationToken = default) =>
        MapPage(await shipments.SearchAsync(filter with { SenderUserId = null, CourierUserId = null }, cancellationToken));

    public async Task<ShipmentDetailDto> GetOwnedAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var shipment = await RequiredAsync(id, cancellationToken);
        return shipment.SenderUserId == userId ? MapDetail(shipment) : throw new ForbiddenException("You cannot access this shipment.");
    }

    public async Task<ShipmentDetailDto> GetAssignedAsync(Guid courierUserId, Guid id, CancellationToken cancellationToken = default)
    {
        var shipment = await RequiredAsync(id, cancellationToken);
        return shipment.CourierUserId == courierUserId ? MapDetail(shipment) : throw new ForbiddenException("This shipment is not assigned to you.");
    }

    public async Task<ShipmentDetailDto> GetAnyAsync(Guid id, CancellationToken cancellationToken = default) =>
        MapDetail(await RequiredAsync(id, cancellationToken));

    public async Task<PublicTrackingDto> TrackPublicAsync(string trackingCode, CancellationToken cancellationToken = default)
    {
        var normalized = trackingCode.Trim().ToUpperInvariant();
        var shipment = await shipments.GetByTrackingCodeAsync(normalized, cancellationToken)
            ?? throw new NotFoundException("No shipment was found for this tracking code.");
        var parts = shipment.RecipientName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var recipient = string.Join(' ', parts.Select(part => $"{part[0]}."));
        return new PublicTrackingDto(
            shipment.TrackingCode,
            recipient,
            $"{shipment.DestinationAddress.City}, {shipment.DestinationAddress.Province}",
            shipment.Type,
            shipment.ServiceLevel,
            shipment.Status,
            shipment.CreatedAtUtc,
            shipment.TrackingEvents.OrderBy(item => item.OccurredAtUtc).Select(MapEvent).ToArray());
    }

    public async Task CancelAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var shipment = await RequiredAsync(id, cancellationToken);
        if (shipment.SenderUserId != userId) throw new ForbiddenException("You cannot cancel this shipment.");
        shipment.TransitionTo(ShipmentStatus.Cancelled, clock.UtcNow, userId, "Cancelled by customer");
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignCourierAsync(Guid id, Guid courierUserId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        if (!await users.IsInRoleAsync(courierUserId, UserRole.Courier, cancellationToken))
            throw new ValidationException(new Dictionary<string, string[]> { ["courierUserId"] = ["The selected user is not an active courier."] });
        var shipment = await RequiredAsync(id, cancellationToken);
        if (shipment.AssignCourier(courierUserId, adminUserId, clock.UtcNow))
            await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task TransitionAsync(Guid id, TransitionShipmentRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var shipment = await RequiredAsync(id, cancellationToken);
        if (shipment.TransitionTo(request.Status, clock.UtcNow, actorUserId, request.Description, request.Location))
            await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordDeliveryAsync(Guid id, Guid courierUserId, RecordDeliveryRequest request, CancellationToken cancellationToken = default)
    {
        var shipment = await RequiredAsync(id, cancellationToken);
        shipment.RecordDelivery(request.Result, request.Note, clock.UtcNow, courierUserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task InitiateReturnAsync(Guid id, Guid adminUserId, string reason, CancellationToken cancellationToken = default)
    {
        var shipment = await RequiredAsync(id, cancellationToken);
        shipment.InitiateReturn(clock.UtcNow, adminUserId, reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Shipment> RequiredAsync(Guid id, CancellationToken cancellationToken) =>
        await shipments.GetAsync(id, cancellationToken) ?? throw new NotFoundException("Shipment not found.");

    private async Task<string> UniqueTrackingCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = trackingCodes.Create(clock.UtcNow);
            if (!await shipments.TrackingCodeExistsAsync(candidate, cancellationToken)) return candidate;
        }

        throw new ConflictException("A unique tracking code could not be generated. Please retry.");
    }

    private static void Validate(CreateShipmentRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.SenderAddressId == Guid.Empty) errors[nameof(request.SenderAddressId)] = ["A sender address is required."];
        if (string.IsNullOrWhiteSpace(request.RecipientName)) errors[nameof(request.RecipientName)] = ["Recipient name is required."];
        if (string.IsNullOrWhiteSpace(request.RecipientPhone)) errors[nameof(request.RecipientPhone)] = ["Recipient phone is required."];
        if (request.WeightGrams <= 0 || request.WeightGrams > 50_000) errors[nameof(request.WeightGrams)] = ["Weight must be between 0 and 50,000 grams."];
        if (request.Type == ShipmentType.Parcel && request.Dimensions is null) errors[nameof(request.Dimensions)] = ["Parcel dimensions are required."];
        if (errors.Count > 0) throw new ValidationException(errors);
    }

    private static PagedResult<ShipmentSummaryDto> MapPage(PagedResult<Shipment> page) =>
        new(page.Items.Select(MapSummary).ToArray(), page.Page, page.PageSize, page.TotalCount);

    private static ShipmentSummaryDto MapSummary(Shipment value) =>
        new(value.Id, value.TrackingCode, value.RecipientName, value.DestinationAddress.City, value.Type, value.ServiceLevel, value.CalculatedPrice, value.Status, value.CreatedAtUtc, value.CourierUserId);

    private static ShipmentDetailDto MapDetail(Shipment value) =>
        new(
            value.Id,
            value.TrackingCode,
            value.SenderUserId,
            value.RecipientName,
            value.RecipientPhone,
            MapAddress(value.SenderAddress),
            MapAddress(value.DestinationAddress),
            value.Type,
            value.WeightGrams,
            value.Dimensions is null ? null : new DimensionsRequest(value.Dimensions.LengthCm, value.Dimensions.WidthCm, value.Dimensions.HeightCm),
            value.ServiceLevel,
            value.CalculatedPrice,
            value.Status,
            value.CreatedAtUtc,
            value.UpdatedAtUtc,
            value.CourierUserId,
            value.DeliveryResult,
            value.DeliveryNote,
            value.TrackingEvents.OrderBy(item => item.OccurredAtUtc).Select(MapEvent).ToArray());

    private static AddressView MapAddress(AddressSnapshot value) =>
        new(value.Label, value.Line1, value.City, value.Province, value.PostalCode, value.Country);

    private static TrackingEventDto MapEvent(TrackingEvent value) =>
        new(value.Status, value.Description, value.OccurredAtUtc, value.Location);
}

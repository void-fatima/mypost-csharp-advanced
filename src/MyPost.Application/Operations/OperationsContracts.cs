using MyPost.Domain.Shipments;

namespace MyPost.Application.Operations;

public sealed record OperationsOverviewDto(
    int TotalShipments,
    int AwaitingPickup,
    int InTransit,
    int OutForDelivery,
    int Delivered,
    int DeliveryFailed,
    int Returning,
    decimal TotalRevenue);

public interface IOperationsReadService
{
    Task<OperationsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<ShipmentStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);
}

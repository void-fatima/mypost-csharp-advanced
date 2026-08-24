using Microsoft.EntityFrameworkCore;
using MyPost.Application.Operations;
using MyPost.Domain.Shipments;
using MyPost.Infrastructure.Persistence;

namespace MyPost.Infrastructure.Operations;

internal sealed class OperationsReadService(MyPostDbContext dbContext) : IOperationsReadService
{
    public async Task<OperationsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var counts = await GetStatusCountsAsync(cancellationToken);
        var total = counts.Values.Sum();
        var revenue = await dbContext.Shipments.Where(item => item.Status != ShipmentStatus.Cancelled).SumAsync(item => item.CalculatedPrice, cancellationToken);
        return new OperationsOverviewDto(
            total,
            Count(ShipmentStatus.AwaitingPickup),
            Count(ShipmentStatus.InTransit),
            Count(ShipmentStatus.OutForDelivery),
            Count(ShipmentStatus.Delivered),
            Count(ShipmentStatus.DeliveryFailed),
            Count(ShipmentStatus.ReturnInitiated) + Count(ShipmentStatus.ReturningToSender),
            revenue);

        int Count(ShipmentStatus status) => counts.GetValueOrDefault(status);
    }

    public async Task<IReadOnlyDictionary<ShipmentStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Shipments.AsNoTracking().GroupBy(item => item.Status).ToDictionaryAsync(group => group.Key, group => group.Count(), cancellationToken);
}

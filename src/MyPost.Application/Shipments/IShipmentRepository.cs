using MyPost.Application.Common;
using MyPost.Domain.Shipments;

namespace MyPost.Application.Shipments;

public interface IShipmentRepository
{
    Task<Shipment?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Shipment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken = default);
    Task<Shipment?> GetByCustomerReferenceAsync(Guid senderUserId, string customerReference, CancellationToken cancellationToken = default);
    Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken cancellationToken = default);
    Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task<PagedResult<Shipment>> SearchAsync(ShipmentFilter filter, CancellationToken cancellationToken = default);
}

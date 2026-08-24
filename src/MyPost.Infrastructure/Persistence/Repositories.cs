using Microsoft.EntityFrameworkCore;
using MyPost.Application.Addresses;
using MyPost.Application.Common;
using MyPost.Application.Shipments;
using MyPost.Domain.Addresses;
using MyPost.Domain.Shipments;

namespace MyPost.Infrastructure.Persistence;

internal sealed class AddressRepository(MyPostDbContext dbContext) : IAddressRepository
{
    public Task<Address?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Addresses.SingleOrDefaultAsync(address => address.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Address>> ListOwnedAsync(Guid ownerUserId, CancellationToken cancellationToken = default) =>
        await dbContext.Addresses.Where(address => address.OwnerUserId == ownerUserId)
            .OrderByDescending(address => address.IsDefault)
            .ThenBy(address => address.Label)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Address address, CancellationToken cancellationToken = default) =>
        await dbContext.Addresses.AddAsync(address, cancellationToken);

    public void Remove(Address address) => dbContext.Addresses.Remove(address);
}

internal sealed class ShipmentRepository(MyPostDbContext dbContext) : IShipmentRepository
{
    private IQueryable<Shipment> Complete => dbContext.Shipments
        .Include(shipment => shipment.TrackingEvents)
        .Include(shipment => shipment.Assignments);

    public Task<Shipment?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Complete.SingleOrDefaultAsync(shipment => shipment.Id == id, cancellationToken);

    public Task<Shipment?> GetByTrackingCodeAsync(string trackingCode, CancellationToken cancellationToken = default) =>
        Complete.SingleOrDefaultAsync(shipment => shipment.TrackingCode == trackingCode, cancellationToken);

    public Task<Shipment?> GetByCustomerReferenceAsync(Guid senderUserId, string customerReference, CancellationToken cancellationToken = default) =>
        Complete.SingleOrDefaultAsync(shipment => shipment.SenderUserId == senderUserId && shipment.CustomerReference == customerReference, cancellationToken);

    public Task<bool> TrackingCodeExistsAsync(string trackingCode, CancellationToken cancellationToken = default) =>
        dbContext.Shipments.AnyAsync(shipment => shipment.TrackingCode == trackingCode, cancellationToken);

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default) =>
        await dbContext.Shipments.AddAsync(shipment, cancellationToken);

    public async Task<PagedResult<Shipment>> SearchAsync(ShipmentFilter filter, CancellationToken cancellationToken = default)
    {
        var query = Complete.AsNoTracking().AsQueryable();
        if (filter.SenderUserId is { } senderUserId) query = query.Where(item => item.SenderUserId == senderUserId);
        if (filter.CourierUserId is { } courierUserId) query = query.Where(item => item.CourierUserId == courierUserId);
        if (filter.Status is { } status) query = query.Where(item => item.Status == status);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(item => item.TrackingCode.Contains(search) || item.RecipientName.Contains(search));
        }

        var total = await query.CountAsync(cancellationToken);
        var page = filter.PageRequest;
        var items = await query.OrderByDescending(item => item.CreatedAtUtc)
            .Skip((page.SafePage - 1) * page.SafePageSize)
            .Take(page.SafePageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<Shipment>(items, page.SafePage, page.SafePageSize, total);
    }
}

using MyPost.Domain.Addresses;

namespace MyPost.Application.Addresses;

public interface IAddressRepository
{
    Task<Address?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Address>> ListOwnedAsync(Guid ownerUserId, CancellationToken cancellationToken = default);
    Task AddAsync(Address address, CancellationToken cancellationToken = default);
    void Remove(Address address);
}

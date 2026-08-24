using MyPost.Application.Abstractions;
using MyPost.Application.Common;
using MyPost.Domain.Addresses;

namespace MyPost.Application.Addresses;

public sealed class AddressService(IAddressRepository addresses, IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<AddressDto>> ListAsync(Guid ownerUserId, CancellationToken cancellationToken = default) =>
        (await addresses.ListOwnedAsync(ownerUserId, cancellationToken)).Select(Map).ToArray();

    public async Task<AddressDto> CreateAsync(Guid ownerUserId, UpsertAddressRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var address = new Address(ownerUserId, request.Label, request.Line1, request.City, request.Province, request.PostalCode, request.IsDefault);
        await addresses.AddAsync(address, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(address);
    }

    public async Task<AddressDto> UpdateAsync(Guid ownerUserId, Guid id, UpsertAddressRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);
        var address = await OwnedAsync(ownerUserId, id, cancellationToken);
        address.Update(request.Label, request.Line1, request.City, request.Province, request.PostalCode);
        address.SetDefault(request.IsDefault);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(address);
    }

    public async Task DeleteAsync(Guid ownerUserId, Guid id, CancellationToken cancellationToken = default)
    {
        var address = await OwnedAsync(ownerUserId, id, cancellationToken);
        addresses.Remove(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Address> OwnedAsync(Guid ownerUserId, Guid id, CancellationToken cancellationToken)
    {
        var address = await addresses.GetAsync(id, cancellationToken) ?? throw new NotFoundException("Address not found.");
        return address.OwnerUserId == ownerUserId ? address : throw new ForbiddenException("You cannot access this address.");
    }

    private static void Validate(UpsertAddressRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Label)) errors[nameof(request.Label)] = ["Label is required."];
        if (string.IsNullOrWhiteSpace(request.Line1)) errors[nameof(request.Line1)] = ["Address line is required."];
        if (string.IsNullOrWhiteSpace(request.City)) errors[nameof(request.City)] = ["City is required."];
        if (string.IsNullOrWhiteSpace(request.Province)) errors[nameof(request.Province)] = ["Province is required."];
        if (string.IsNullOrWhiteSpace(request.PostalCode)) errors[nameof(request.PostalCode)] = ["Postal code is required."];
        if (errors.Count > 0) throw new ValidationException(errors);
    }

    private static AddressDto Map(Address value) =>
        new(value.Id, value.Label, value.Line1, value.City, value.Province, value.PostalCode, value.IsDefault);
}

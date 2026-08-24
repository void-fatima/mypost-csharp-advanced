namespace MyPost.Application.Addresses;

public sealed record AddressDto(
    Guid Id,
    string Label,
    string Line1,
    string City,
    string Province,
    string PostalCode,
    bool IsDefault);

public sealed record UpsertAddressRequest(
    string Label,
    string Line1,
    string City,
    string Province,
    string PostalCode,
    bool IsDefault = false);

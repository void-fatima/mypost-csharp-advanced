using MyPost.Domain.Common;
using MyPost.Domain.Shipments;

namespace MyPost.Domain.Addresses;

public sealed class Address
{
    private Address() { }

    public Address(Guid ownerUserId, string label, string line1, string city, string province, string postalCode, bool isDefault = false)
    {
        if (ownerUserId == Guid.Empty)
        {
            throw new DomainException("An address owner is required.");
        }

        Id = Guid.NewGuid();
        OwnerUserId = ownerUserId;
        Update(label, line1, city, province, postalCode);
        IsDefault = isDefault;
    }

    public Guid Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Line1 { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Province { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    public void Update(string label, string line1, string city, string province, string postalCode)
    {
        var snapshot = new AddressSnapshot(label, line1, city, province, postalCode);
        Label = snapshot.Label;
        Line1 = snapshot.Line1;
        City = snapshot.City;
        Province = snapshot.Province;
        PostalCode = snapshot.PostalCode;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SetDefault(bool value) => IsDefault = value;

    public AddressSnapshot Snapshot() => new(Label, Line1, City, Province, PostalCode);
}

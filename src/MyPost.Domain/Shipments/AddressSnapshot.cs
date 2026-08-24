using MyPost.Domain.Common;

namespace MyPost.Domain.Shipments;

public sealed record AddressSnapshot
{
    private AddressSnapshot() { }

    public AddressSnapshot(string label, string line1, string city, string province, string postalCode, string country = "Iran")
    {
        Label = Guard.Required(label, nameof(label), 80);
        Line1 = Guard.Required(line1, nameof(line1), 240);
        City = Guard.Required(city, nameof(city), 100);
        Province = Guard.Required(province, nameof(province), 100);
        PostalCode = Guard.Required(postalCode, nameof(postalCode), 20);
        Country = Guard.Required(country, nameof(country), 80);
    }

    public string Label { get; private init; } = string.Empty;
    public string Line1 { get; private init; } = string.Empty;
    public string City { get; private init; } = string.Empty;
    public string Province { get; private init; } = string.Empty;
    public string PostalCode { get; private init; } = string.Empty;
    public string Country { get; private init; } = string.Empty;
}

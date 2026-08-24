using MyPost.Domain.Common;

namespace MyPost.Domain.Shipments;

public sealed record Dimensions
{
    private Dimensions() { }

    public Dimensions(decimal lengthCm, decimal widthCm, decimal heightCm)
    {
        if (lengthCm <= 0 || widthCm <= 0 || heightCm <= 0)
        {
            throw new DomainException("All dimensions must be greater than zero.");
        }

        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
    }

    public decimal LengthCm { get; private init; }
    public decimal WidthCm { get; private init; }
    public decimal HeightCm { get; private init; }
}

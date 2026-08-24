using MyPost.Domain.Shipments;

namespace MyPost.Application.Shipments;

public interface IShipmentPriceCalculator
{
    decimal Calculate(ShipmentType type, decimal weightGrams, Dimensions? dimensions, ServiceLevel serviceLevel);
}

public sealed class ShipmentPriceCalculator : IShipmentPriceCalculator
{
    public decimal Calculate(ShipmentType type, decimal weightGrams, Dimensions? dimensions, ServiceLevel serviceLevel)
    {
        var basePrice = type == ShipmentType.Letter ? 45_000m : 85_000m;
        var weightPrice = Math.Ceiling(weightGrams / 500m) * (type == ShipmentType.Letter ? 8_000m : 16_000m);
        var volumePrice = dimensions is null ? 0m : Math.Ceiling((dimensions.LengthCm * dimensions.WidthCm * dimensions.HeightCm) / 5_000m) * 12_000m;
        var multiplier = serviceLevel switch
        {
            ServiceLevel.Economy => 0.9m,
            ServiceLevel.Express => 1.65m,
            _ => 1m
        };

        return decimal.Round((basePrice + weightPrice + volumePrice) * multiplier, 0, MidpointRounding.AwayFromZero);
    }
}

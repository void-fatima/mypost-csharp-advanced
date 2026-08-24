namespace ConsoleApp.Models;

public class Home : Data<string>
{
    public string PostalCode { get; set; } = string.Empty;
    public string OwnerNationalCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Meterage { get; set; }
    public string Address { get; set; } = string.Empty;

    // DONT remove default constructor.
    public Home() { }

    public Home(string ownerNationalCode, decimal price, string postalCode, double meterage, string address)
    {
        OwnerNationalCode = ownerNationalCode;
        Price = price;
        PostalCode = postalCode;
        Meterage = meterage;
        Address = address;
        Key = postalCode;
    }

    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(Address)
               && System.Text.RegularExpressions.Regex.IsMatch(OwnerNationalCode ?? string.Empty, @"^\d{6}$")
               && System.Text.RegularExpressions.Regex.IsMatch(PostalCode ?? string.Empty, @"^\d{6}$")
               && Price > 0
               && Meterage > 0
               && !double.IsNaN(Meterage)
               && !double.IsInfinity(Meterage);
    }

    public static Dictionary<string, Home> LoadFromFile(string filePath)
    {
        var homes = JsonFileStorage.LoadDictionary<Home>(
            filePath,
            home => home.PostalCode,
            home => home.Validate());

        foreach (var home in homes.Values)
        {
            home.Key = home.PostalCode;
        }

        return homes;
    }

    public static void SaveToFile(List<Home> homes, string filePath)
    {
        JsonFileStorage.Save(homes, filePath);
    }
}

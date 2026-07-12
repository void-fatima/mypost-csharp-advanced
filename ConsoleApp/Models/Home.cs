namespace ConsoleApp.Models;

public class Home : Data<string>
{
    public string PostalCode { get; set; }
    public string OwnerNationalCode { get; set; }
    public decimal Price { get; set; }
    public double Meterage { get; set; }
    public string Address { get; set; }

    // DONT remove default constructor.
    public Home() { }

    public Home(string ownerNationalCode, decimal price, string postalCode, double meterage, string address)
    {
        throw new NotImplementedException();
    }

    public bool Validate()
    {
        throw new NotImplementedException();
    }
   
    public static Dictionary<string, Home> LoadFromFile(string filePath)
    {
        throw new NotImplementedException();
    }
    
    public static void SaveToFile(List<Home> homes, string filePath)
    {
        throw new NotImplementedException();
    }
}
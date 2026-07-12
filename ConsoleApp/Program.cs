using System.Text.Json;
using ConsoleApp.Models;

namespace ConsoleApp;

class Program
{
    static void Main(string[] args)
    {
        // Define the data folder path
        var dataFolderPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Data");
            
        // Create sample JSON files (temporary function)
        CreateSampleDataFiles(dataFolderPath);

        // Output file path
        var outputParcelPostFile = Path.Combine(dataFolderPath, "ParcelPost.json");

        // Initialize PostService
        var postService = new PostService(dataFolderPath);

        // Process letters to create parcel queue
        postService.ProcessLetters();

        // Save parcel queue to file
        postService.SaveState(outputParcelPostFile);

        Console.WriteLine("ParcelPost data has been saved to " + outputParcelPostFile);
    }

    // Temporary function to create sample JSON files
    static void CreateSampleDataFiles(string dataFolderPath)
    {
        // Ensure the Data directory exists
        if (!Directory.Exists(dataFolderPath))
        {
            Directory.CreateDirectory(dataFolderPath);
        }

        // Sample People data
        var people = new List<Person>
        {
            new("John", "Doe", "123456", "1990-01-01", "CityA", "09123456789", "john.doe@example.com"),
            new("Jane", "Smith", "654321", "1985-05-15", "CityB", "09198765432", "jane.smith@example.com"),
            new("Invalid", "User", "ABCDEF", "2000-12-31", "CityC", "0901234567", "invalidemail") // Invalid data
        };

        // Sample Homes data
        var homes = new List<Home>
        {
            new("123456", 250000.00m, "111111", 120.5, "123 Main St, CityA"),
            new("654321", 300000.00m, "222222", 150.0, "456 Elm St, CityB")
        };

        // Sample Letters data
        var letters = new List<Letter>
        {
            new("Hello, how are you?", "Jane Smith", "John Doe", "LETTER123"),
            new("Meeting reminder.", "John Doe", "Jane Smith", "LETTER456"),
            new("Unknown receiver.", "Unknown Person", "John Doe", "LETTER789")
        };

        // Serialize and save People.json
        string peopleJson = JsonSerializer.Serialize(people, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(dataFolderPath, "People.json"), peopleJson);

        // Serialize and save Houses.json
        string homesJson = JsonSerializer.Serialize(homes, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(dataFolderPath, "Houses.json"), homesJson);

        // Serialize and save Letters.json
        string lettersJson = JsonSerializer.Serialize(letters, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(dataFolderPath, "Letters.json"), lettersJson);

        Console.WriteLine("Sample data files have been created in the Data folder.");
    }
}
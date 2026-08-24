using ConsoleApp.Models;

namespace ConsoleApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        var dataFolderPath = args.Length > 0
            ? Path.GetFullPath(args[0])
            : Path.Combine(AppContext.BaseDirectory, "Data");

        CreateSampleDataFiles(dataFolderPath);

        var postService = new PostService(dataFolderPath);
        postService.ProcessLetters();

        var outputPath = Path.Combine(dataFolderPath, "ParcelPost.json");
        postService.SaveState(outputPath);

        var parcels = postService.GetParcels();
        Console.WriteLine($"Processed {parcels.Count} letter(s); {parcels.Count(parcel => parcel.IsReturned)} returned.");
        Console.WriteLine($"Parcel data was saved to: {outputPath}");
    }

    private static void CreateSampleDataFiles(string dataFolderPath)
    {
        Directory.CreateDirectory(dataFolderPath);

        var people = new List<Person>
        {
            new("John", "Doe", "123456", "1990-01-01", "CityA", "09123456789", "john.doe@example.com"),
            new("Jane", "Smith", "654321", "1985-05-15", "CityB", "09198765432", "jane.smith@example.com"),
            new("Invalid", "User", "ABCDEF", "2000-12-31", "CityC", "0901234567", "invalidemail")
        };

        var homes = new List<Home>
        {
            new("123456", 250000.00m, "111111", 120.5, "123 Main St, CityA"),
            new("654321", 300000.00m, "222222", 150.0, "456 Elm St, CityB")
        };

        var letters = new List<Letter>
        {
            new("Hello, how are you?", "Jane Smith", "John Doe", "LETTER123"),
            new("Meeting reminder.", "John Doe", "Jane Smith", "LETTER456"),
            new("Unknown receiver.", "Unknown Person", "John Doe", "LETTER789")
        };

        var peoplePath = Path.Combine(dataFolderPath, "People.json");
        var homesPath = Path.Combine(dataFolderPath, "Houses.json");
        var lettersPath = Path.Combine(dataFolderPath, "Letters.json");

        SaveIfMissing(peoplePath, () => Person.SaveToFile(people, peoplePath));
        SaveIfMissing(homesPath, () => Home.SaveToFile(homes, homesPath));
        SaveIfMissing(lettersPath, () => Letter.SaveToFile(letters, lettersPath));
    }

    private static void SaveIfMissing(string filePath, Action save)
    {
        if (!File.Exists(filePath))
        {
            save();
        }
    }
}

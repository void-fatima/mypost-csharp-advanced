using System.Text.Json;

namespace ConsoleApp.Models;

internal static class JsonFileStorage
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    public static Dictionary<string, T> LoadDictionary<T>(
        string filePath,
        Func<T, string> keySelector,
        Func<T, bool> validator)
        where T : class
    {
        EnsureFilePath(filePath);

        var result = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(filePath))
        {
            return result;
        }

        var json = File.ReadAllText(filePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        List<T>? items;
        try
        {
            items = JsonSerializer.Deserialize<List<T>>(json, ReadOptions);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"The JSON file '{filePath}' is not valid.", exception);
        }

        if (items is null)
        {
            return result;
        }

        foreach (var item in items)
        {
            if (item is null || !validator(item))
            {
                continue;
            }

            var key = keySelector(item);
            if (!string.IsNullOrWhiteSpace(key))
            {
                result[key] = item;
            }
        }

        return result;
    }

    public static void Save<T>(IEnumerable<T> items, string filePath)
    {
        ArgumentNullException.ThrowIfNull(items);
        EnsureFilePath(filePath);

        var fullPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(items, WriteOptions);
        File.WriteAllText(fullPath, json);
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A file path is required.", nameof(filePath));
        }
    }
}

using ConsoleApp.Models;

namespace ConsoleApp.Tests;

[TestClass]
public class ModelTests
{
    [TestMethod]
    public void PersonValidation_RejectsInvalidContactInformation()
    {
        var valid = CreatePerson("123456", "john@example.com");
        var invalid = CreatePerson("ABCDEF", "not-an-email");

        Assert.IsTrue(valid.Validate());
        Assert.IsFalse(invalid.Validate());
    }

    [TestMethod]
    public void PersonPersistence_LoadsOnlyValidRecords()
    {
        using var directory = new TemporaryDirectory();
        var filePath = Path.Combine(directory.Path, "People.json");
        var valid = CreatePerson("123456", "john@example.com");
        var invalid = CreatePerson("ABCDEF", "not-an-email");

        Person.SaveToFile(new List<Person> { valid, invalid }, filePath);
        var loaded = Person.LoadFromFile(filePath);

        Assert.AreEqual(1, loaded.Count);
        Assert.AreEqual(valid.NationalCode, loaded[valid.NationalCode].NationalCode);
    }

    [TestMethod]
    public void MissingJsonFile_ReturnsAnEmptyDictionary()
    {
        using var directory = new TemporaryDirectory();

        var loaded = Letter.LoadFromFile(Path.Combine(directory.Path, "missing.json"));

        Assert.AreEqual(0, loaded.Count);
    }

    private static Person CreatePerson(string nationalCode, string email) =>
        new("John", "Doe", nationalCode, "1990-01-01", "Tehran", "09123456789", email);
}

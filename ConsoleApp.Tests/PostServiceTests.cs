using System.Text.Json;
using ConsoleApp.Models;

namespace ConsoleApp.Tests;

[TestClass]
public class PostServiceTests
{
    [TestMethod]
    public void AddMethods_RejectInvalidAndDuplicateRecords()
    {
        var service = new PostService();
        var person = CreatePerson("John", "Doe", "123456");
        var invalidPerson = CreatePerson("Invalid", "Person", "ABCDEF");
        var home = new Home("123456", 100m, "111111", 50, "Tehran");
        var letter = new Letter("Hello", "John Doe", "Jane Doe", "L1");

        Assert.IsTrue(service.AddPerson(person));
        Assert.IsFalse(service.AddPerson(person));
        Assert.IsFalse(service.AddPerson(invalidPerson));
        Assert.IsTrue(service.AddHome(home));
        Assert.IsFalse(service.AddHome(home));
        Assert.IsTrue(service.AddLetter(letter));
        Assert.IsFalse(service.AddLetter(letter));
    }

    [TestMethod]
    public void ProcessLetters_DeliversKnownReceiverAndReturnsUnknownReceiver()
    {
        var service = new PostService();
        var sender = CreatePerson("John", "Doe", "123456");
        var receiver = CreatePerson("Jane", "Smith", "654321");

        service.AddPerson(sender);
        service.AddPerson(receiver);
        service.AddHome(new Home(sender.NationalCode, 100m, "111111", 50, "Sender address"));
        service.AddHome(new Home(receiver.NationalCode, 200m, "222222", 70, "Receiver address"));
        service.AddLetter(new Letter("Delivered", "  jane   smith ", "John Doe", "L1"));
        service.AddLetter(new Letter("Returned", "Unknown Person", "John Doe", "L2"));

        service.ProcessLetters();
        service.ProcessLetters();
        var parcels = service.GetParcels().ToArray();

        Assert.AreEqual(2, parcels.Length);
        Assert.IsFalse(parcels[0].IsReturned);
        Assert.AreEqual("Receiver address", parcels[0].Address);
        Assert.AreEqual("222222", parcels[0].ReceiverPostalCode);
        Assert.IsTrue(parcels[1].IsReturned);
        Assert.AreEqual("Sender address", parcels[1].Address);
        Assert.AreEqual("111111", parcels[1].SenderPostalCode);
    }

    [TestMethod]
    public void SaveState_WritesTheCurrentQueueAsJson()
    {
        using var directory = new TemporaryDirectory();
        var service = new PostService();
        var person = CreatePerson("John", "Doe", "123456");
        service.AddPerson(person);
        service.AddHome(new Home(person.NationalCode, 100m, "111111", 50, "Home"));
        service.AddLetter(new Letter("Hello", "John Doe", "John Doe", "L1"));
        service.ProcessLetters();

        var filePath = Path.Combine(directory.Path, "ParcelPost.json");
        service.SaveState(filePath);
        var saved = JsonSerializer.Deserialize<List<ParcelPost>>(File.ReadAllText(filePath));

        Assert.IsNotNull(saved);
        Assert.AreEqual(1, saved.Count);
        Assert.AreEqual("L1", saved[0].Letter.LetterId);
    }

    private static Person CreatePerson(string firstName, string lastName, string nationalCode) =>
        new(firstName, lastName, nationalCode, "1990-01-01", "Tehran", "09123456789", $"{nationalCode}@example.com");
}

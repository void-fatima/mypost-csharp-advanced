using ConsoleApp.Models;

namespace ConsoleApp;

public class PostService
{
    // DONT change this properties.
    private Dictionary<string, Person> personDict;
    private Dictionary<string, Home> homeDict;
    private Dictionary<string, Letter> letterDict;
    private Queue<ParcelPost> parcelQueue;
    public Queue<ParcelPost> Parcels => parcelQueue;

    public PostService()
    {
        // Initialize dictionaries and parcel queue
        personDict = new Dictionary<string, Person>();
        homeDict = new Dictionary<string, Home>();
        letterDict = new Dictionary<string, Letter>();
        parcelQueue = new Queue<ParcelPost>();
    }

    public PostService(string dataFolderPath)
    {
        if (string.IsNullOrWhiteSpace(dataFolderPath))
        {
            throw new ArgumentException("A data folder path is required.", nameof(dataFolderPath));
        }

        var peopleFilePath = Path.Combine(dataFolderPath, "People.json");
        var homesFilePath = Path.Combine(dataFolderPath, "Houses.json");
        var lettersFilePath = Path.Combine(dataFolderPath, "Letters.json");

        // Load data
        personDict = Person.LoadFromFile(peopleFilePath);
        homeDict = Home.LoadFromFile(homesFilePath);
        letterDict = Letter.LoadFromFile(lettersFilePath);
        parcelQueue = new Queue<ParcelPost>();
    }

    public void ProcessLetters()
    {
        parcelQueue.Clear();

        foreach (var letter in letterDict.Values)
        {
            parcelQueue.Enqueue(CreateParcelPost(letter));
        }
    }

    public void SaveState(string filePath)
    {
        ParcelPost.SaveToFile(parcelQueue, filePath);
    }

    public bool AddPerson(Person person)
    {
        if (person is null || !person.Validate() || personDict.ContainsKey(person.NationalCode))
        {
            return false;
        }

        personDict.Add(person.NationalCode, person);
        return true;
    }

    public bool AddHome(Home home)
    {
        if (home is null || !home.Validate() || homeDict.ContainsKey(home.PostalCode))
        {
            return false;
        }

        home.Key = home.PostalCode;
        homeDict.Add(home.PostalCode, home);
        return true;
    }

    public bool AddLetter(Letter letter)
    {
        if (letter is null || !letter.Validate() || letterDict.ContainsKey(letter.LetterId))
        {
            return false;
        }

        letterDict.Add(letter.LetterId, letter);
        return true;
    }

    private ParcelPost CreateParcelPost(Letter letter)
    {
        var receiver = FindPersonByName(letter.ReceiverFullName);
        var sender = FindPersonByName(letter.SenderFullName);
        var receiverHome = receiver is null ? null : FindHomeByOwnerNationalCode(receiver.NationalCode);
        var senderHome = sender is null ? null : FindHomeByOwnerNationalCode(sender.NationalCode);
        var isReturned = receiver is null || receiverHome is null;
        var destinationHome = isReturned ? senderHome : receiverHome;

        return new ParcelPost(
            letter,
            letter.ReceiverFullName,
            destinationHome?.Address ?? string.Empty,
            receiverHome?.PostalCode ?? string.Empty,
            senderHome?.PostalCode ?? string.Empty,
            isReturned);
    }

    private Person? FindPersonByName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return null;
        }

        var normalizedName = NormalizeName(fullName);
        return personDict.Values.FirstOrDefault(person =>
            string.Equals(
                NormalizeName($"{person.FirstName} {person.LastName}"),
                normalizedName,
                StringComparison.OrdinalIgnoreCase));
    }

    private Home? FindHomeByOwnerNationalCode(string nationalCode)
    {
        if (string.IsNullOrWhiteSpace(nationalCode))
        {
            return null;
        }

        return homeDict.Values.FirstOrDefault(home =>
            string.Equals(home.OwnerNationalCode, nationalCode, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeName(string value) =>
        string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    // DO NOT REMOVE THIS METHOD
    public Queue<ParcelPost> GetParcels()
    {
        return new Queue<ParcelPost>(parcelQueue);
    }
}

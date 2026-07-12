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
        var peopleFilePath = Path.Combine(dataFolderPath, "People.json");
        var homesFilePath = Path.Combine(dataFolderPath, "Houses.json");
        var lettersFilePath = Path.Combine(dataFolderPath, "Letters.json");

        // Load data
        personDict = Person.LoadFromFile(peopleFilePath);
        homeDict = Home.LoadFromFile(homesFilePath);
        letterDict = Letter.LoadFromFile(lettersFilePath);
    }

    public void ProcessLetters()
    {
        throw new NotImplementedException();
    }

    public void SaveState(string filePath)
    {
        throw new NotImplementedException();
    }
    
    public bool AddPerson(Person person)
    {
        throw new NotImplementedException();
    }

    public bool AddHome(Home home)
    {
        throw new NotImplementedException();
    }

    public bool AddLetter(Letter letter)
    {
        throw new NotImplementedException();
    }

    private ParcelPost CreateParcelPost(Letter letter)
    {
        throw new NotImplementedException();
    }

    private Person FindPersonByName(string fullName)
    {
        throw new NotImplementedException();
    }

    private Home FindHomeByOwnerNationalCode(string nationalCode)
    {
        throw new NotImplementedException();
    }

    // DO NOT REMOVE THIS METHOD
    public Queue<ParcelPost> GetParcels()
    {
        return new Queue<ParcelPost>(parcelQueue);
    }
}
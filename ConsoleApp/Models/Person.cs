namespace ConsoleApp.Models;

public class Person
{
    public string NationalCode { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string BirthDate { get; set; }
    public string BirthPlace { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }

    // DONT remove default constructor
    public Person() { }
    
    public Person(string firstName, string lastName, string nationalCode, string birthDate, string birthPlace, string phoneNumber, string email)
    {
        throw new NotImplementedException();
    }

    public bool Validate()
    {
        throw new NotImplementedException();
    }

    public static Dictionary<string, Person> LoadFromFile(string filePath)
    {
        throw new NotImplementedException();
    }
    
    public static void SaveToFile(List<Person> persons, string filePath)
    {
        throw new NotImplementedException();
    }
}
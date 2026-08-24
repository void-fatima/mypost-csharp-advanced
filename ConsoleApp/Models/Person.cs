using System.Text.RegularExpressions;

namespace ConsoleApp.Models;

public class Person
{
    public string NationalCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string BirthDate { get; set; } = string.Empty;
    public string BirthPlace { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // DONT remove default constructor
    public Person() { }

    public Person(string firstName, string lastName, string nationalCode, string birthDate, string birthPlace, string phoneNumber, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        NationalCode = nationalCode;
        BirthDate = birthDate;
        BirthPlace = birthPlace;
        PhoneNumber = phoneNumber;
        Email = email;
    }

    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(FirstName)
               && !string.IsNullOrWhiteSpace(LastName)
               && !string.IsNullOrWhiteSpace(BirthDate)
               && !string.IsNullOrWhiteSpace(BirthPlace)
               && Regex.IsMatch(NationalCode ?? string.Empty, @"^\d{6}$")
               && Regex.IsMatch(PhoneNumber ?? string.Empty, @"^09\d{9}$")
               && Regex.IsMatch(Email ?? string.Empty, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    public static Dictionary<string, Person> LoadFromFile(string filePath)
    {
        return JsonFileStorage.LoadDictionary<Person>(
            filePath,
            person => person.NationalCode,
            person => person.Validate());
    }

    public static void SaveToFile(List<Person> persons, string filePath)
    {
        JsonFileStorage.Save(persons, filePath);
    }
}

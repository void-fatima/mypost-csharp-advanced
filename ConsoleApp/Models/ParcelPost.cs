namespace ConsoleApp.Models;

public class ParcelPost
{
    public Letter Letter { get; set; } = null!;
    public string Name { get; set; } = string.Empty; // Receiver's full name
    public string Address { get; set; } = string.Empty;
    public string ReceiverPostalCode { get; set; } = string.Empty;
    public string SenderPostalCode { get; set; } = string.Empty;
    public bool IsReturned { get; set; }

    // DONT remove default constructor
    public ParcelPost() { }

    public ParcelPost(Letter letter, string name, string address, string receiverPostalCode, string senderPostalCode, bool isReturned)
    {
        Letter = letter ?? throw new ArgumentNullException(nameof(letter));
        Name = name;
        Address = address;
        ReceiverPostalCode = receiverPostalCode;
        SenderPostalCode = senderPostalCode;
        IsReturned = isReturned;
    }

    public static void SaveToFile(Queue<ParcelPost> parcelQueue, string filePath)
    {
        JsonFileStorage.Save(parcelQueue, filePath);
    }
}

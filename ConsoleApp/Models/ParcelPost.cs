namespace ConsoleApp.Models;

public class ParcelPost
{
    public Letter Letter { get; set; }
    public string Name { get; set; } // Receiver's full name
    public string Address { get; set; }
    public string ReceiverPostalCode { get; set; }
    public string SenderPostalCode { get; set; }
    public bool IsReturned { get; set; }

    // DONT remove default constructor
    public ParcelPost() { }

    public ParcelPost(Letter letter, string name, string address, string receiverPostalCode, string senderPostalCode, bool isReturned)
    {
        throw new NotImplementedException();
    }

    public static void SaveToFile(Queue<ParcelPost> parcelQueue, string filePath)
    {
        throw new NotImplementedException();
    }
}
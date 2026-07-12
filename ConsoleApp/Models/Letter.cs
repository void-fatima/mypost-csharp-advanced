namespace ConsoleApp.Models;

public class Letter
{
    public string LetterId { get; set; }
    public string Text { get; set; }
    public string ReceiverFullName { get; set; }
    public string SenderFullName { get; set; }
    
    public Letter(string text, string receiverFullName, string senderFullName, string letterId)
    {
        throw new NotImplementedException();
    }

    public bool Validate()
    {
        throw new NotImplementedException();
    }

    public static Dictionary<string, Letter> LoadFromFile(string filePath)
    {
        throw new NotImplementedException();
    }
    
    public static void SaveToFile(List<Letter> letters, string filePath)
    {
        throw new NotImplementedException();
    }
}
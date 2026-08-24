namespace ConsoleApp.Models;

public class Letter
{
    public string LetterId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string ReceiverFullName { get; set; } = string.Empty;
    public string SenderFullName { get; set; } = string.Empty;

    public Letter(string text, string receiverFullName, string senderFullName, string letterId)
    {
        Text = text;
        ReceiverFullName = receiverFullName;
        SenderFullName = senderFullName;
        LetterId = letterId;
    }

    public bool Validate()
    {
        return !string.IsNullOrWhiteSpace(LetterId)
               && !string.IsNullOrWhiteSpace(Text)
               && !string.IsNullOrWhiteSpace(ReceiverFullName)
               && !string.IsNullOrWhiteSpace(SenderFullName);
    }

    public static Dictionary<string, Letter> LoadFromFile(string filePath)
    {
        return JsonFileStorage.LoadDictionary<Letter>(
            filePath,
            letter => letter.LetterId,
            letter => letter.Validate());
    }

    public static void SaveToFile(List<Letter> letters, string filePath)
    {
        JsonFileStorage.Save(letters, filePath);
    }
}

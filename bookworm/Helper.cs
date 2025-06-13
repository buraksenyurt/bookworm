namespace bookworm;

public static class Helper
{
    public static void ShowMessage(MessageType messageType, string[] messages)
    {
        if (messages == null || messages.Length == 0)
            return;

        string message = string.Join("\n", messages);
        var originalColor = Console.ForegroundColor;

        ConsoleColor newColor;
        string prefix;

        switch (messageType)
        {
            case MessageType.Info:
                newColor = ConsoleColor.Green;
                prefix = "Info";
                break;
            case MessageType.Warning:
                newColor = ConsoleColor.Yellow;
                prefix = "Warning";
                break;
            case MessageType.Error:
                newColor = ConsoleColor.Red;
                prefix = "Error";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
        }

        Console.ForegroundColor = newColor;
        Console.WriteLine($"{prefix}: {message}");
        Console.ForegroundColor = originalColor;
    }
}

public enum MessageType
{
    Info,
    Warning,
    Error
}

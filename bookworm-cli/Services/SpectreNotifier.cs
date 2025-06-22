using Spectre.Console;

namespace Services;

public class SpectreNotifier
    :INotifier
{
    public void ShowMessage(MessageType messageType, string[] messages)
    {
        if (messages == null || messages.Length == 0)
            return;

        string message = string.Join("\n", messages);
        string color;
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        switch (messageType)
        {
            case MessageType.Info:
                color = "bold green";
                AnsiConsole.MarkupLine(Emoji.Known.BeatingHeart + " [bold green]Info[/] :beating_heart:");
                break;
            case MessageType.Warning:
                color = "bold yellow";
                AnsiConsole.MarkupLine(Emoji.Known.Warning + " [bold yellow]Warnning[/] :warning:");
                break;
            case MessageType.Error:
                color = "bold red";
                AnsiConsole.MarkupLine(Emoji.Known.CrossMark + " [bold red]Error[/] :cross_mark:");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
        }
        AnsiConsole.MarkupLineInterpolated($"[{color}]{message}[/]");
    }
}

public enum MessageType
{
    Info,
    Warning,
    Error
}

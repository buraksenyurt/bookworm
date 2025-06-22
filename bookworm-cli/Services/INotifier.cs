namespace Services;

public interface INotifier
{
    void ShowMessage(MessageType messageType, string[] messages);
}

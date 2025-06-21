namespace Services;

public interface IMessageWriter
{
    void ShowMessage(MessageType messageType, string[] messages);
}

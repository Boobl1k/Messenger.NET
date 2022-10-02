namespace Back.Contracts;

public class MessageContract
{
    public MessageContract(string userName, string text)
    {
        UserName = userName;
        Text = text;
    }

    public string UserName { get; set; }
    public string Text { get; set; }
}
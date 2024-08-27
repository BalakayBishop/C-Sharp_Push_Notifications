public class SendPushRequest
{
    public string UserIds { get; set; }
    public string App { get; set; }
    public string Title { get; set; }
    public string Body { get; set; } = "";
    public string ClickAction { get; set; } = "/";
}
public class PushSubscription
{
    public string Endpoint { get; set; }
    public Keys Keys { get; set; }
}

public class Keys
{
    public string P256dh { get; set; }
    public string Auth { get; set; }
}
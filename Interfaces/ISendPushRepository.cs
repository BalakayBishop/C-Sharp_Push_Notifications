public interface ISendPushRepository
{
    Task SendPushAsync(SendPushRequest pushRequest);
}
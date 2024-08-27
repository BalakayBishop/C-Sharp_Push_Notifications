public class SendPushRepository : ISendPushRepository
{
    private readonly IConfiguration _configuration;
    private readonly Security _security;
    private readonly string _signingPayload;
    private readonly string _appKey;
    private readonly string _baseUri;

    public SendPushRepository(IConfiguration configuration, Security security)
    {
        _configuration = configuration;
        _security = security;
        _signingPayload = _configuration["NotificationSigning"];
        _appKey = _configuration["AppKey"];
        _baseUri = _configuration["ApiBaseUri"];
    }

    public async Task SendPushAsync(SendPushRequest pushRequest)
    {
        string token = await _security.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            string signature = await _security.SignPayloadAsync(_signingPayload);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_baseUri);
                client.DefaultRequestHeaders.Add("X-Signature", signature);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.PostAsJsonAsync("api/Notifications/SendPush", pushRequest);
            }
        }
    }
}
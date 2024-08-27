
private readonly IConfiguration _configuration;
private readonly SignInManager<IdentityUser> _signInManager;
private readonly UserManager<IdentityUser> _userManager;
private readonly IEmailSenderRepository _emailSenderRepository;
private readonly IUserRepository _userRepository;
private readonly IDistrictRepository _districtRepository;
private readonly Security _security;

private readonly string _signingPayload;
private readonly string _appKey;

public AccountController(IConfiguration configuration, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IEmailSenderRepository emailSenderRepository, IUserRepository userRepository, IDistrictRepository districtRepository, Security security)
{
    _userRepository = userRepository;
    _districtRepository = districtRepository;
    _signInManager = signInManager;
    _userManager = userManager;
    _configuration = configuration;
    _emailSenderRepository = emailSenderRepository;
    _security = security;

    _signingPayload = _configuration["NotificationSigning"];
    _appKey = _configuration["AppKey"];
}

#region ***** PUSH NOTIFICATIONS *****

/// <summary>
/// Creates or updates the push subscription for the current user.
/// </summary>
/// <param name="subscription">The push subscription details.</param>
/// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateOrUpdatePushSubscription([FromBody] PushSubscription subscription)
{
    // Get the user's identity id
    UserViewModel user = await _userRepository.GetByEmail(HttpContext.User.Identity.Name);
    string userIdentityId = user.IdentityId;

    // Get JWT from API
    string token = await _security.GetToken();

    if (!string.IsNullOrEmpty(token))
    {
        // get the signature for the request
        string signature = await _security.SignPayloadAsync(_signingPayload);

        // encrypt and serialize the user identity id for api endpoint
        userIdentityId = await _security.EncryptAsync(userIdentityId);
        userIdentityId = JsonConvert.SerializeObject(userIdentityId);

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = new Uri(_configuration["ApiBaseUri"]);
            client.DefaultRequestHeaders.Add("X-Signature", signature);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Serializing object to pass as string to API
            string serializedSubscription = JsonConvert.SerializeObject(subscription);

            CreateOrUpdateRequestBody requestBody = new()
            {
                Json = serializedSubscription,
                App = _configuration["AppName"]!,
                UserId = userIdentityId
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("api/PushNotifications/CreateOrUpdate", requestBody);

            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(errorMessage);
            }
        }
    }

    return BadRequest("Token error");
}

/// <summary>
/// Deletes the push subscription for the current user.
/// </summary>
/// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
[HttpDelete]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeletePushSubscription()
{
    UserViewModel user = await _userRepository.GetByEmail(HttpContext.User.Identity.Name);
    string userIdentityId = user.IdentityId;

    // Get JWT from API
    string token = await _security.GetToken();

    if (!string.IsNullOrEmpty(token))
    {
        // get the signature for the request
        string signature = await _security.SignPayloadAsync(_signingPayload);

        // encrypt and serialize the user identity id for api endpoint
        userIdentityId = await _security.EncryptAsync(userIdentityId);
        userIdentityId = JsonConvert.SerializeObject(userIdentityId);

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = new Uri(_configuration["ApiBaseUri"]);
            client.DefaultRequestHeaders.Add("X-Signature", signature);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            DeleteSubscriptionRequestBody requestBody = new()
            {
                App = _configuration["AppName"]!,
                UserId = userIdentityId
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("api/PushNotifications/DeleteSubscription", requestBody);

            if (response.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                return BadRequest(errorMessage);
            }
        }
    }

    return BadRequest("Token error");
}

#endregion
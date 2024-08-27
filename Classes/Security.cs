public class Security
{
    private readonly IConfiguration _configuration;
    private readonly KeyClient _keyClient;
    private readonly SecretClient _secretClient;
    private CryptographyClient _cryptoClient;
    private CryptographyClient _signingCryptoClient;

    private readonly string _keyVaultUrl;
    private readonly string _encryptionKeyName;
    private readonly string _signingKeyName;
    private readonly string _appKey;
    private readonly string _baseUri;

    public Security(IConfiguration configuration)
    {
        _configuration = configuration;

        _keyVaultUrl = _configuration["KeyVaultUrl"]!;
        _encryptionKeyName = _configuration["AzureKeyVault:EncryptionKey"]!;
        _signingKeyName = _configuration["AzureKeyVault:SigningKey"]!;

        DefaultAzureCredential credential = new DefaultAzureCredential();

        _keyClient = new KeyClient(new Uri(_keyVaultUrl), credential);

        Azure.Response<KeyVaultKey> encryptionKey = _keyClient.GetKey(_encryptionKeyName);
        _cryptoClient = new CryptographyClient(encryptionKey.Value.Id, credential);

        Azure.Response<KeyVaultKey> signingKey = _keyClient.GetKey(_signingKeyName);
        _signingCryptoClient = new CryptographyClient(signingKey.Value.Id, credential);

        _appKey = _configuration["AppKey"];
        _baseUri = _configuration["ApiBaseUri"];
    }

    /// <summary>
    /// Gets the authentication token from the Notifications API.
    /// </summary>
    /// <returns>The authentication JWT token.</returns>
    public async Task<string> GetToken()
    {
        string token = string.Empty;

        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = new Uri(_baseUri);
            var requestBody = new StringContent(JsonConvert.SerializeObject(_appKey), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("api/Authentication/GetToken", requestBody);

            if (response.IsSuccessStatusCode)
            {
                token = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<TokenResponse>(token).Token;
            }
            else
            {
                return token = null!;
            }

            return token;
        }
    }

    /// <summary>
    /// Encrypts the specified plaintext using RSA-OAEP encryption algorithm.
    /// </summary>
    /// <param name="plaintext">The plain text to encrypt.</param>
    /// <returns>The encrypted cipher text as a Base64 string.</returns>
    public async Task<string> EncryptAsync(string plaintext)
    {
        byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        EncryptResult encryptResult = await _cryptoClient.EncryptAsync(EncryptionAlgorithm.RsaOaep, plaintextBytes);
        return Convert.ToBase64String(encryptResult.Ciphertext);
    }

    /// <summary>
    /// Decrypts the specified cipher text using RSA-OAEP decryption algorithm.
    /// </summary>
    /// <param name="ciphertext">The cipher text to decrypt.</param>
    /// <returns>The decrypted plain text.</returns>
    public async Task<string> DecryptAsync(string ciphertext)
    {
        byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);
        DecryptResult decryptResult = await _cryptoClient.DecryptAsync(EncryptionAlgorithm.RsaOaep, ciphertextBytes);
        return Encoding.UTF8.GetString(decryptResult.Plaintext);
    }

    /// <summary>
    /// Signs the specified encrypted payload using the RS256 signature algorithm.
    /// </summary>
    /// <param name="encryptedPayload">The encrypted payload to sign.</param>
    /// <returns>The base64-encoded signature of the encrypted payload.</returns>
    public async Task<string> SignPayloadAsync(string encryptedPayload)
    {
        byte[] payloadBytes = Encoding.UTF8.GetBytes(encryptedPayload);
        SignResult signResult = await _signingCryptoClient.SignAsync(SignatureAlgorithm.RS256, payloadBytes);
        return Convert.ToBase64String(signResult.Signature);
    }

    /// <summary>
    /// Verifies the signature of the specified encrypted payload using the RS256 signature algorithm.
    /// </summary>
    /// <param name="encryptedPayload">The encrypted payload to verify.</param>
    /// <param name="signature">The base64-encoded signature of the encrypted payload.</param>
    /// <returns>True if the signature is valid; otherwise, false.</returns>
    public async Task<bool> VerifySignatureAsync(string encryptedPayload, string signature)
    {
        byte[] payloadBytes = Encoding.UTF8.GetBytes(encryptedPayload);
        byte[] signatureBytes = Convert.FromBase64String(signature);
        VerifyResult verifyResult = await _signingCryptoClient.VerifyAsync(SignatureAlgorithm.RS256, payloadBytes, signatureBytes);
        return verifyResult.IsValid;
    }
}
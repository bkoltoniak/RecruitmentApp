using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecruitmentApp.Core.Extensions;
using RecruitmentApp.Core.Interfaces;
using RecruitmentApp.Core.Options;
using System.Security.Cryptography;
using System.Text;

namespace RecruitmentApp.Core.Services;

public class ApiKeySerivce : IApiKeyService
{
    private const int HashLenght = 32;

    private readonly IOptions<ApiKeyOptions> _configuration;
    private readonly ILogger<ApiKeySerivce> _logger;

    public ApiKeySerivce(IOptions<ApiKeyOptions> configuration, ILogger<ApiKeySerivce> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // Generates key using HMACSHA256 algorithm with creation timestamp as message. Returns Base64 encoded key.
    public string GetKey()
    {
        if (string.IsNullOrWhiteSpace(_configuration.Value.Secret))
        {
            throw new("Secret value cannot be empty.");
        }

        var secret = Encoding.UTF8.GetBytes(_configuration.Value.Secret);
        using var hmacProvider = new HMACSHA256(secret);

        var timeStamp = DateTime.UtcNow.ToUnixTimestamp();
        var expirity = BitConverter.GetBytes(timeStamp);

        var hash = hmacProvider.ComputeHash(expirity);
        var hmacValue = hash.Concat(expirity).ToArray();

        return Convert.ToBase64String(hmacValue);
    }

    // Validates key against HMACSHA256 and timestamp.
    public bool ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new("Key cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(_configuration.Value.Secret))
        {
            throw new("Secret value cannot be empty.");
        }

        var hmacValue = Convert.FromBase64String(key);
        var keyHash = hmacValue.Take(HashLenght).ToArray();
        var keyExpirity = hmacValue.Skip(HashLenght).ToArray();
        int timeStamp = BitConverter.ToInt32(keyExpirity);

        var secret = Encoding.UTF8.GetBytes(_configuration.Value.Secret);
        using var hmacProvider = new HMACSHA256(secret);
        var computedHash = hmacProvider.ComputeHash(keyExpirity);

        return computedHash.SequenceEqual(keyHash) && ValidateExpirity(timeStamp);
    }

    private bool ValidateExpirity(int timeStamp) =>
        timeStamp > DateTime.UtcNow.AddMinutes(_configuration.Value.ExpireMinutes * -1).ToUnixTimestamp();
}
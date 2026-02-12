using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection;
using System.Xml.Linq;

namespace ti8m.BeachBreak.Infrastructure;

/// <summary>
/// Monitors and logs data protection key rotation events for security auditing.
/// </summary>
public class DataProtectionKeyRotationLogger : IKeyEscrowSink
{
    private readonly ILogger logger;

    public DataProtectionKeyRotationLogger(ILogger<Program> logger)
    {
        this.logger = logger;
    }

    public void Store(Guid keyId, XElement element)
    {
        try
        {
            var keyInfo = ExtractKeyInformation(element);

            logger.LogInformation("Data Protection Key Rotation Event: " +
                "KeyId={KeyId}, CreationDate={CreationDate}, ActivationDate={ActivationDate}, " +
                "ExpirationDate={ExpirationDate}, Algorithm={Algorithm}",
                keyId,
                keyInfo.CreationDate,
                keyInfo.ActivationDate,
                keyInfo.ExpirationDate,
                keyInfo.Algorithm);

            // Log security-relevant information
            logger.LogInformation("Key Rotation Security Audit: KeyId={KeyId}, " +
                "Environment={Environment}, MachineName={MachineName}, Timestamp={Timestamp}",
                keyId,
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                Environment.MachineName,
                DateTimeOffset.UtcNow);

            // Check for potential issues
            if (keyInfo.ExpirationDate.HasValue && keyInfo.ExpirationDate.Value < DateTimeOffset.UtcNow.AddDays(30))
            {
                logger.LogWarning("Data Protection Key expires within 30 days: " +
                    "KeyId={KeyId}, ExpirationDate={ExpirationDate}",
                    keyId, keyInfo.ExpirationDate.Value);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging data protection key rotation for KeyId={KeyId}", keyId);
        }
    }

    private static KeyInformation ExtractKeyInformation(XElement element)
    {
        var creationDate = DateTimeOffset.TryParse(
            element.Attribute("creationDate")?.Value,
            out var creation) ? creation : (DateTimeOffset?)null;

        var activationDate = DateTimeOffset.TryParse(
            element.Attribute("activationDate")?.Value,
            out var activation) ? activation : (DateTimeOffset?)null;

        var expirationDate = DateTimeOffset.TryParse(
            element.Attribute("expirationDate")?.Value,
            out var expiration) ? expiration : (DateTimeOffset?)null;

        var algorithm = element.Element("descriptor")
            ?.Element("encryption")
            ?.Attribute("algorithm")?.Value ?? "Unknown";

        return new KeyInformation(creationDate, activationDate, expirationDate, algorithm);
    }

    private record KeyInformation(
        DateTimeOffset? CreationDate,
        DateTimeOffset? ActivationDate,
        DateTimeOffset? ExpirationDate,
        string Algorithm);
}
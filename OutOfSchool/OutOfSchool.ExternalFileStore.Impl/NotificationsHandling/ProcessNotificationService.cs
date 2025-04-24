using Microsoft.Extensions.Logging;
using OutOfSchool.ExternalFileStore.NotificationInterfaces;
using System.Text.Json;

namespace OutOfSchool.ExternalFileStore.NotificationImplementations;
public class ProcessNotificationService : IProcessNotificationService
{
    private readonly ILogger<ProcessNotificationService> _logger;    

    public ProcessNotificationService(ILogger<ProcessNotificationService> logger)
    {
        _logger = logger;        
    }

    /// <inheritdoc/>
    public void ProcessNotification(string message)
    {
        try
        {
            _logger.LogInformation("Processing notification: {MessageStart}...",
                message.Length > 50 ? message.Substring(0, 50) + "..." : message);

            var jsonDoc = JsonDocument.Parse(message);
            var records = jsonDoc.RootElement.GetProperty("Records");

            foreach (var record in records.EnumerateArray())
            {
                var eventName = record.GetProperty("eventName").GetString();
                var eventTime = record.GetProperty("eventTime").GetString();
                var objectKey = record.GetProperty("s3").GetProperty("object").GetProperty("key").GetString();
                var bucketName = record.GetProperty("s3").GetProperty("bucket").GetProperty("name").GetString();

                _logger.LogInformation(
                    "Processed notification - Bucket: {Bucket}, Key: {Key}, Event: {Event}, Time: {Time}",
                    bucketName,
                    objectKey,
                    eventName,
                    eventTime);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification: {Message}", ex.Message);
        }
    }
}

using Microsoft.Extensions.Logging;
using Minio.DataModel.Notification;
using OutOfSchool.ExternalFileStore.Models;
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
            var notification = JsonSerializer.Deserialize<StorageNotificationEvent>(message);
            if (notification != null)
            {
                _ = HandleObjectCreatedAsync(notification);
            }
            else
            {
                _logger.LogWarning("Received null notification after deserializing message: {Message}", message);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize notification message: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", message);
        }
    }

    private async Task HandleObjectCreatedAsync(StorageNotificationEvent notification)
    {
        _logger.LogInformation("Object created: {BucketName}/{Key}",
            notification.BucketName, notification.Key);
        await Task.CompletedTask;
    }

    // <inheritdoc/>
    public StorageNotificationEvent ParseMinioNotification(MinioNotificationRaw notification)
    {
        try
        {
            _logger.LogDebug("Parsing MinIO notification: {Notification}", notification.Json);

            var jsonDoc = JsonDocument.Parse(notification.Json);
            var root = jsonDoc.RootElement;
            
            if (!root.TryGetProperty("Records", out var records) || records.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning("Notification JSON does not contain Records array: {Json}", notification.Json);
                return null;
            }

            // Process the first record (usually there's only one per notification)
            var record = records[0];

            // Extract event data
            var eventNameString = record.GetProperty("eventName").GetString();

            // Skip if it's not an ObjectCreated event
            if (!eventNameString.StartsWith("s3:ObjectCreated:"))
            {
                _logger.LogDebug("Ignoring non-ObjectCreated event: {EventName}", eventNameString);
                return null;
            }

            var eventTime = record.GetProperty("eventTime").GetDateTime();

            // Extract S3 object information
            var s3 = record.GetProperty("s3");
            var bucket = s3.GetProperty("bucket").GetProperty("name").GetString();

            // The object key may contain URL encoding
            var objectKey = s3.GetProperty("object").GetProperty("key").GetString();
            objectKey = Uri.UnescapeDataString(objectKey); // Decode URL-encoded characters
            
            return new StorageNotificationEvent
            {
                BucketName = bucket,
                Key = objectKey,
                EventTime = eventTime
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse MinIO notification JSON: {Notification}", notification.Json);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error parsing MinIO notification: {Notification}", notification.Json);
            return null;
        }
    }
}

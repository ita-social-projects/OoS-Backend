namespace OutOfSchool.BusinessLogic.Services.ThumbnailProcessor;
public interface IThumbnailProcessingService
{
    Task<bool> ProcessImage(string imageId);
    Task ProcessAllUnprocessedThumbnailsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasThumbnail(string imageId);
}

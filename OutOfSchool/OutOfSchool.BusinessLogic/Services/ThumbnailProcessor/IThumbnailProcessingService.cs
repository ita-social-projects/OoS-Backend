namespace OutOfSchool.BusinessLogic.Services.ThumbnailProcessor;
public interface IThumbnailProcessingService
{
    Task<bool> ProcessImage(string imageId);
    Task<bool> HasThumbnail(string imageId);
}

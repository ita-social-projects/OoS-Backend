namespace OutOfSchool.BusinessLogic.Config.Images;
public class UploadConcurrencySettings
{
    /// <summary>
    /// The maximum number of concurrent image uploads allowed.
    /// Adjust this value to balance performance and resource usage.
    /// </summary>
    public int MaxParallelImageUploads { get; set; } = 4;
}

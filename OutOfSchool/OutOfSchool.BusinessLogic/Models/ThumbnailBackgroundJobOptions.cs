namespace OutOfSchool.BusinessLogic.Models;
public class ThumbnailBackgroundJobOptions
{
    public bool Enabled { get; set; }
    public int BatchSize { get; set; } = 100;
}

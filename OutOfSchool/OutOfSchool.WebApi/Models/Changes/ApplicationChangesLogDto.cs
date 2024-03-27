namespace OutOfSchool.WebApi.Models.Changes;

public class ApplicationChangesLogDto : ChangesLogDtoBase
{
    public Guid ApplicationId { get; set; }

    public string WorkshopTitle { get; set; }

    public string WorkshopCity { get; set; }

    public string ProviderTitle { get; set; }
}
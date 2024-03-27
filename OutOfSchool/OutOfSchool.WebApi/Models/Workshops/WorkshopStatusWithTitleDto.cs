namespace OutOfSchool.WebApi.Models.Workshops;

public class WorkshopStatusWithTitleDto : WorkshopStatusDto
{
    public string Title { get; set; } = string.Empty;
}

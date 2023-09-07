namespace OutOfSchool.WebApi.Models.Workshop;

public class WorkshopStatusWithTitleDTO : WorkshopStatusDTO
{
    public string Title { get; set; } = string.Empty;
}

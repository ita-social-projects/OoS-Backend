namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopStatusWithTitleDto : WorkshopStatusDto
{
    public string Title { get; set; } = string.Empty;
}

public static class WorkshopStatusWithTitleDtoExtensions
{
    public static WorkshopStatusWithTitleDto ToWorkshopStatusWithTitleDto(this WorkshopStatusDto dto, string title)
        => new()
        {
            WorkshopId = dto.WorkshopId,
            Status = dto.Status,
            Title = title
        };
}

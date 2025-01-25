namespace OutOfSchool.BusinessLogic.Models.Workshops.V2;

public class WorkshopV2Dto : WorkshopDto
{
    public string CoverImageId { get; set; } = string.Empty;

    public IList<string> ImageIds { get; set; }
}

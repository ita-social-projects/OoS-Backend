using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Workshops;

public class WorkshopDescriptionItemInfo
{
    [MaxLength(200)]
    public string SectionName { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }
}

public static class WorkshopDescriptionItemInfoExtensions
{
    public static WorkshopDescriptionItemInfo ToInfo(this WorkshopDescriptionItem model)
        => new()
        {
            SectionName = model.SectionName,
            Description = model.Description,
        };

    public static List<WorkshopDescriptionItemInfo> ToInfo(this IEnumerable<WorkshopDescriptionItem> list)
        => list.MapToList(ToInfo);

    public static List<WorkshopDescriptionItemInfo> ToNotDeletedInfo(this IEnumerable<WorkshopDescriptionItem> list)
        => list.MapNonDeletedToList(ToInfo);
}
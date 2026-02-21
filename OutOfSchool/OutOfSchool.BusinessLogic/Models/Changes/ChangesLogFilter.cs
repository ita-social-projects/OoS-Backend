using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Changes;

public class ChangesLogFilter : ChangesLogFilterBase
{
    [Required]
    public string EntityType { get; set; }
}

public static class ChangesLogFilterExtensions
{
    public static ChangesLogFilter ToFilter(this ProviderChangesLogRequest providerChanges)
        => new()
        {
            EntityId = providerChanges.EntityId,
            PropertyName = providerChanges.PropertyName,
            DateFrom = providerChanges.DateFrom,
            DateTo = providerChanges.DateTo,
            EntityType = "Provider",
            Size = 0
        };

    public static ChangesLogFilter ToFilter(this ApplicationChangesLogRequest applicationChanges)
        => new()
        {
            EntityId = applicationChanges.EntityId,
            PropertyName = applicationChanges.PropertyName,
            DateFrom = applicationChanges.DateFrom,
            DateTo = applicationChanges.DateTo,
            EntityType = "Application",
            Size = 0
        };

    public static List<ChangesLogFilter> ToFilter(this IEnumerable<ProviderChangesLogRequest> list)
        => list.MapToList(ToFilter);

    public static ChangesLogFilter ToFilter(this WorkshopDraftChangesLogRequest request)
    => new()
    {
        EntityId = request.EntityId,
        PropertyName = request.PropertyName,
        DateFrom = request.DateFrom,
        DateTo = request.DateTo,
        EntityType = "WorkshopDraft",
        SearchString = request.SearchString,
        From = request.From,
        Size = request.Size
    };

}
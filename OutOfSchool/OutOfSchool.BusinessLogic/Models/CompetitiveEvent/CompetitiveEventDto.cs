using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

public class CompetitiveEventDto : CompetitiveEventBaseDto
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
    public uint Rating { get; set; } = 0;
    public uint NumberOfRatings { get; set; } = 0;
    public string InstitutionHierarchy { get; set; }
    public List<long> DirectionIds { get; set; }
    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }
    public CompetitiveEventCoverageDto Coverage { get; set; }
}
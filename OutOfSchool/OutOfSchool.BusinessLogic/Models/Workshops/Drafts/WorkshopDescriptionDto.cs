using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.BusinessLogic.Models.Workshops.Drafts;

public class WorkshopDescriptionDto : WorkshopRequiredPropertiesDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description item is required")]
    public IEnumerable<WorkshopDescriptionItemDto> WorkshopDescriptionItems { get; set; }

    public bool WithDisabilityOptions { get; set; } = default;

    [MaxLength(200)]
    public string DisabilityOptionsDesc { get; set; } = string.Empty;

    public Guid? InstitutionId { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public List<long> DirectionIds { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<string> Keywords { get; set; } = default;

    [MaxLength(500)]
    public string EnrollmentProcedureDescription { get; set; }

    public bool AreThereBenefits { get; set; } = default;

    [MaxLength(500)]
    public string PreferentialTermsOfParticipation { get; set; }

    [EnumDataType(typeof(Coverage), ErrorMessage = Constants.EnumErrorMessage)]
    public Coverage Coverage { get; set; } = Coverage.School;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> TagIds { get; set; } = [];
}

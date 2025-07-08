using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums.Workshop;
using static OutOfSchool.BusinessLogic.Validators.ConditionalValidationAttributes;

namespace OutOfSchool.BusinessLogic.Models.Workshops.TempSave;

public class WorkshopDescriptionDto : WorkshopRequiredPropertiesDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description item is required")]
    public IEnumerable<WorkshopDescriptionItemDto> WorkshopDescriptionItems { get; set; }

    public Guid? InstitutionId { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public bool IsChampionPath { get; set; } = false;

    public List<long> DirectionIds { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<string> Keywords { get; set; } = default;

    [MaxLength(500)]
    public string EnrollmentProcedureDescription { get; set; }

    [EnumDataType(typeof(Coverage), ErrorMessage = Constants.EnumErrorMessage)]
    public Coverage Coverage { get; set; } = Coverage.School;

    [ConditionalRequired("EnableWorkshopTags")]
    [ConditionalMinLength("EnableWorkshopTags", 3, ErrorMessage = "At least three tags are required")]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> TagIds { get; set; } = [];
}

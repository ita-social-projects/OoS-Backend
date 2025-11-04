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

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IEnumerable<string> Keywords { get; set; } = default;

    [MaxLength(Constants.EnrollmentProcedureDescription)]
    public string EnrollmentProcedureDescription { get; set; }

    [EnumDataType(typeof(Coverage), ErrorMessage = Constants.EnumErrorMessage)]
    public Coverage Coverage { get; set; } = Coverage.School;

    [ConditionalRequired("EnableWorkshopTags")]
    [ConditionalMinLength("EnableWorkshopTags", 3, ErrorMessage = "At least three tags are required")]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> TagIds { get; set; } = [];

    [Required(ErrorMessage = "Property CompetitiveSelection is required")]
    public bool CompetitiveSelection { get; set; }

    [MaxLength(Constants.MaxCompetitiveSelectionDescriptionLength)]
    public string CompetitiveSelectionDescription { get; set; }

    // This property uses only for storing dto in Redis
    [ConditionalMinLength("Images", 1, ErrorMessage = "At least one image is required")]
    [ConditionalMaxLength("Images", 10, ErrorMessage = "The image collection must contain less than 10 items")]
    public List<string> Base64ImageFiles { get; set; }
}

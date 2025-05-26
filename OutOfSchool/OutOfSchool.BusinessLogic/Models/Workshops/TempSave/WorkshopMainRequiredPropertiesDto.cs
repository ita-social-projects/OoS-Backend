using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Workshops.TempSave;

[JsonDerivedType(typeof(WorkshopMainRequiredPropertiesDto), typeDiscriminator: "withMainProperties")]
[JsonDerivedType(typeof(WorkshopRequiredPropertiesDto), typeDiscriminator: "withOtherRequiredProperties")]
[JsonDerivedType(typeof(WorkshopDescriptionDto), typeDiscriminator: "withDescription")]
[JsonDerivedType(typeof(WorkshopContactsDto), typeDiscriminator: "withContacts")]
[JsonDerivedType(typeof(WorkshopCreateRequestDto), typeDiscriminator: "withStaff")]
[JsonDerivedType(typeof(WorkshopV2CreateRequestDto), typeDiscriminator: "withImages")]
public class WorkshopMainRequiredPropertiesDto : IValidatableObject
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(Constants.MinWorkshopTitleLength)]
    [MaxLength(Constants.MaxWorkshopTitleLength)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;
    public bool NoAgeRestrictions { get; set; } = false;

    [RequiredIf("NoAgeRestrictions", false, ErrorMessage = "Min age is required when there are age restrictions")]
    [Range(0, 120, ErrorMessage = "Min age should be between 0 and 120")]
    public int? MinAge { get; set; }

    [RequiredIf("NoAgeRestrictions", false, ErrorMessage = "Max age is required when there are age restrictions")]
    [Range(0, 120, ErrorMessage = "Max age should be between 0 and 120")]
    public int? MaxAge { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one DateTime range is required")]
    public List<DateTimeRangeDto> DateTimeRanges { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [Required(ErrorMessage = "Study period dates is required")]
    public StudyPeriodDatesDto StudyPeriodDates { get; set; }

    [Required(ErrorMessage = "Form of learning is required")]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning FormOfLearning { get; set; } = FormOfLearning.Offline;

    [Required(ErrorMessage = "Available seats are required")]
    public uint? AvailableSeats { get; set; } = uint.MaxValue;

    [Required(ErrorMessage = "Property CompetitiveSelection is required")]
    public bool CompetitiveSelection { get; set; }

    [MaxLength(500)]
    public string CompetitiveSelectionDescription { get; set; }

    [Required]
    public Guid ProviderId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // TODO: Validate DateTimeRanges are not empty when frontend is ready
        foreach (var dateTimeRange in DateTimeRanges)
        {
            if (dateTimeRange.StartTime >= dateTimeRange.EndTime)
            {
                yield return new ValidationResult(
                    "The end date cannot be equal to or earlier than the start date");
            }

            if (dateTimeRange.Workdays.IsNullOrEmpty() || dateTimeRange.Workdays.Any(workday => workday == DaysBitMask.None))
            {
                yield return new ValidationResult(
                    "Workdays are required");
            }

            var daysHs = new HashSet<DaysBitMask>();
            if (!dateTimeRange.Workdays.All(daysHs.Add))
            {
                yield return new ValidationResult(
                    "Workdays contain duplications");
            }
        }
        if (NoAgeRestrictions)
        {
            MinAge = 0;
            MaxAge = 120;
        }
        else if (MinAge.HasValue && MaxAge.HasValue && MinAge > MaxAge)
        {
            yield return new ValidationResult("Min age should be less than or equal to Max age", new[] { nameof(MinAge), nameof(MaxAge) });
        }
    }
}
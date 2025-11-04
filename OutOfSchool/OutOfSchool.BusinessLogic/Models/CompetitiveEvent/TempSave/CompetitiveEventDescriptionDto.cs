using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums;
using static OutOfSchool.BusinessLogic.Validators.ConditionalValidationAttributes;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.TempSave;

public class CompetitiveEventDescriptionDto : CompetitiveEventAboutDto
{
    // This property uses only for storing dto in Redis
    [ConditionalMinLength("Images", 1, ErrorMessage = "At least one image is required")]
    [ConditionalMaxLength("Images", 10, ErrorMessage = "The image collection must contain less than 10 items")]
    public List<string> Base64ImageFiles { get; set; } = [];

    [FromForm]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description item is required")]
    public List<CompetitiveEventDescriptionItemDto> CompetitiveEventDescriptionItems { get; set; } = [];

    [Required]
    public int CoverageId { get; set; }

    [FromForm]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> SubDirectionIds { get; set; } = [];

    [Required(ErrorMessage = "Planned format of classes is required")]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning? PlannedFormatOfClasses { get; set; }

    public bool? CompetitiveSelection { get; set; }

    [MinLength(3)]
    [MaxLength(Constants.MaxCompetitiveSelectionDescriptionLength)]
    [RequiredIf(nameof(CompetitiveSelection), true, ErrorMessage = "Competitive selection description is required")]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string CompetitiveSelectionDescription { get; set; }

    [MinLength(3)]
    [MaxLength(Constants.MaxVenueNameLength)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string VenueName { get; set; }

    [MinLength(3)]
    [MaxLength(Constants.EnrollmentProcedureDescription)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 0 to 100 000")]
    public int? Price { get; set; }

    public bool? AreThereBenefits { get; set; }

    [MinLength(3)]
    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    [RequiredIf(nameof(AreThereBenefits), true, ErrorMessage = "Benefits is required")]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string Benefits { get; set; }

    public IList<DirectionSubDirectionIdsDto> DirectionSubDirectionIds { get; set; } = [];

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from CompetitiveEventAboutDto
        foreach (var error in base.Validate(validationContext))
            yield return error;
    }
}
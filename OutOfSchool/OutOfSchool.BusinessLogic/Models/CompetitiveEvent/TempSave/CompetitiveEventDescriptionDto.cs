using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static OutOfSchool.BusinessLogic.Validators.ConditionalValidationAttributes;
using static OutOfSchool.BusinessLogic.Validators.RequiredIfMinAndMaxLengthAttributes;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.TempSave;

public class CompetitiveEventDescriptionDto : CompetitiveEventAboutDto
{
    // This property uses only for storing dto in Redis
    [ConditionalMinLength("Images", 1, ErrorMessage = "At least one image is required")]
    [ConditionalMaxLength("Images", 10, ErrorMessage = "A maximum of 10 images are allowed per competitive event.")]
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

    [Required]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning? PlannedFormatOfClasses { get; set; }

    public bool? CompetitiveSelection { get; set; }

    [RequiredIf(nameof(CompetitiveSelection), true, ErrorMessage = "Competitive selection description is required")]
    [RequiredIfMinLength(nameof(CompetitiveSelection), true, Constants.MinCompetitiveSelectionDescriptionLength, ErrorMessage = "Competitive selection description must contain at least 3 letters.")]
    [RequiredIfMaxLength(nameof(CompetitiveSelection), true, Constants.MaxCompetitiveSelectionDescriptionLength, ErrorMessage = "Competitive selection description must not contain greater than 2000 letters.")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Competitive selection description must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string CompetitiveSelectionDescription { get; set; }

    [MinLength(Constants.MinVenueNameLength)]
    [MaxLength(Constants.MaxVenueNameLength)]
    [MustContain(RequiredCharacterType.AnyLetter)]
    public string VenueName { get; set; }

    [Required]
    [MinLength(Constants.MinLengthOfDescriptionOfTheEnrollmentProcedureForCompetitiveEvent)]
    [MaxLength(Constants.MaxLengthOfDescriptionOfTheEnrollmentProcedureForCompetitiveEvent)]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "DescriptionOfTheEnrollmentProcedure field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    public bool IsPaid { get; set; } = false;

    [MaxDecimalPlaces(2, ErrorMessage = "Price field must have maximum two decimal places.")]
    [RequiredIf(nameof(IsPaid), true, ErrorMessage = "Price is required")]
    [ModelBinder(BinderType = typeof(DecimalDotModelBinder))]
    [JsonConverter(typeof(DecimalDotJsonConverter))]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 0 to 100 000")]
    public decimal? Price { get; set; } = default;

    public bool? AreThereBenefits { get; set; }

    [RequiredIf(nameof(AreThereBenefits), true, ErrorMessage = "Benefits is required")]
    [RequiredIfMinLength(nameof(AreThereBenefits), true, Constants.MinBenefitsLength, ErrorMessage = "Benefits must contain at least 3 letters.")]
    [RequiredIfMaxLength(nameof(AreThereBenefits), true, Constants.MaxBenefitsLength, ErrorMessage = "Benefits must not contain greater than 2000 letters.")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Benefits field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string Benefits { get; set; }

    public IList<DirectionSubDirectionIdsDto> DirectionSubDirectionIds { get; set; } = [];

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from CompetitiveEventAboutDto
        foreach (var error in base.Validate(validationContext))
            yield return error;

        // validate Price when IsPaid is true
        if (IsPaid && (!Price.HasValue || Price < 0.01M))
        {
            yield return new ValidationResult("Price must be specified and must be in the range from 0.01 to 100000.00 when the competitive event is paid.", [nameof(Price)]);
        }
    }
}
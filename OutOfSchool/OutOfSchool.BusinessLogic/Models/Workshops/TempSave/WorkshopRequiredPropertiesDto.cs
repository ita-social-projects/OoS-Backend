using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;

namespace OutOfSchool.BusinessLogic.Models.Workshops.TempSave;

public class WorkshopRequiredPropertiesDto : WorkshopMainRequiredPropertiesDto
{
    [Required(ErrorMessage = "Should be indicated if the Workshop operates with funds from parents or benefactors")]
    public bool IsSelfFinanced { get; set; } = false;

    [EnumDataType(typeof(SpecialNeedsType), ErrorMessage = Constants.EnumErrorMessage)]
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    [Required(ErrorMessage = "Property IsInclusive is required")]
    public bool IsInclusive { get; set; } = false;

    [Required(ErrorMessage = "Educational shift is required")]
    [EnumDataType(typeof(EducationalShift), ErrorMessage = Constants.EnumErrorMessage)]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;

    [Required(ErrorMessage = "Type of age composition is required")]
    [EnumDataType(typeof(AgeComposition), ErrorMessage = Constants.EnumErrorMessage)]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;

    [EnumDataType(typeof(WorkshopType), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopType WorkshopType { get; set; } = WorkshopType.Workshop;

    public Guid? ParentWorkshopId { get; set; }

    [Required(ErrorMessage = "Property IsPaid is required")]
    public bool IsPaid { get; set; } = false;

    [MaxDecimalPlaces(2, ErrorMessage = "Price field must have maximum two decimal places.")]
    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 0 to 100 000")]
    [RequiredIf(nameof(IsPaid), true, ErrorMessage = "Price is required")]
    public decimal? Price { get; set; } = default;

    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    public PayRateType? PayRate { get; set; } = PayRateType.None;

    public bool AreThereBenefits { get; set; } = default;

    [MinLength(Constants.MinPreferentialTermsOfParticipationLength)]
    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    [RequiredIf(nameof(AreThereBenefits), true, ErrorMessage = "PreferentialTermsOfParticipation is required")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "PreferentialTermsOfParticipation field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string PreferentialTermsOfParticipation { get; set; }

    public Guid? InstitutionId { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    public bool IsChampionPath { get; set; } = false;

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from WorkshopMainRequiredPropertiesDto
        foreach (var error in base.Validate(validationContext))
            yield return error;

        // validate Price and PayRate when IsPaid is true
        if (IsPaid)
        {
            if (PayRate == null || PayRate == PayRateType.None)
            {
                yield return new ValidationResult("Pay rate must be specified when the workshop is paid.", [nameof(PayRate)]);
            }

            if (!Price.HasValue || Price < 0.01M)
            {
                yield return new ValidationResult("Price must be specified and must be in the range from 0.01 to 100000.00 when the workshop is paid.", [nameof(Price)]);
            }
        }
    }
}
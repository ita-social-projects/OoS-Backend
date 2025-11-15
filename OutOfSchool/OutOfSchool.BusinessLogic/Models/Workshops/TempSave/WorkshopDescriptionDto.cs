using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Validators;
using OutOfSchool.Common.Enums.Workshop;
using static OutOfSchool.BusinessLogic.Validators.ConditionalValidationAttributes;

namespace OutOfSchool.BusinessLogic.Models.Workshops.TempSave;

public class WorkshopDescriptionDto : WorkshopRequiredPropertiesDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one description item is required")]
    public IEnumerable<WorkshopDescriptionItemDto> WorkshopDescriptionItems { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [MaxLength(Constants.MaxCountOfKeywordsForWorkshop)]
    public IEnumerable<string> Keywords { get; set; } = default;

    [Required]
    [MinLength(Constants.MinLengthOfEnrollmentProcedureDescriptionForWorkshop)]
    [MaxLength(Constants.MaxLengthOfEnrollmentProcedureDescriptionForWorkshop)]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "EnrollmentProcedureDescription field must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string EnrollmentProcedureDescription { get; set; }

    [EnumDataType(typeof(Coverage), ErrorMessage = Constants.EnumErrorMessage)]
    public Coverage Coverage { get; set; } = Coverage.School;

    [ConditionalRequired("EnableWorkshopTags")]
    [ConditionalMinLength("EnableWorkshopTags", 3, ErrorMessage = "At least three tags are required")]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<long> TagIds { get; set; } = [];

    [Required(ErrorMessage = "Property CompetitiveSelection is required")]
    public bool CompetitiveSelection { get; set; }

    [MinLength(Constants.MinCompetitiveSelectionDescriptionLength)]
    [MaxLength(Constants.MaxCompetitiveSelectionDescriptionLength)]
    [RequiredIf(nameof(CompetitiveSelection), true, ErrorMessage = "Competitive selection description is required")]
    [MustContain(RequiredCharacterType.AnyLetter, ErrorMessage = "Competitive selection description must contain at least one letter.")]
    [RegularExpression(@"^[\p{IsCyrillic}\p{IsBasicLatin}0-9\s\p{P}\p{S}]+$", ErrorMessage = "Only Cyrillic, Latin, numbers and symbols are allowed.")]
    public string CompetitiveSelectionDescription { get; set; }

    // This property uses only for storing dto in Redis
    [ConditionalMinLength("Images", 1, ErrorMessage = "At least one image is required")]
    [ConditionalMaxLength("Images", 10, ErrorMessage = "A maximum of 10 images are allowed per workshop.")]
    public List<string> Base64ImageFiles { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from WorkshopRequiredPropertiesDto
        foreach (var error in base.Validate(validationContext))
            yield return error;

        if (!Keywords.IsNullOrEmpty())
        {
            var keywordsList = Keywords.ToList();
            var cleanedKeywords = new List<string>(keywordsList.Count);
            int totalLength = 0;

            // Check for null/whitespace, trim, check single length, and calculate total length
            foreach (var keyword in keywordsList)
            {
                // Check 1: Empty or whitespace
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    yield return new ValidationResult("Keyword cannot be empty or whitespace.", [nameof(Keywords)]);
                    continue;
                }

                string trimmedKeyword = keyword.Trim();
                cleanedKeywords.Add(trimmedKeyword);
                totalLength += trimmedKeyword.Length;

                // Check 2: Single keyword max length
                if (trimmedKeyword.Length > Constants.MaxLengthOfOneKeyword)
                {
                    yield return new ValidationResult($"Keyword must be no longer than {Constants.MaxLengthOfOneKeyword} characters.", [nameof(Keywords)]);
                }
            }

            // Check 3: Total length of all keywords
            if (totalLength > Constants.MaxKeywordsLength)
            {
                yield return new ValidationResult(
                    $"The length of all keywords must not exceed {Constants.MaxKeywordsLength} characters.",
                    [nameof(Keywords)]);
            }

            // Check 4: Duplicates (case-insensitive)
            HashSet<string> keywordsSet = new(StringComparer.OrdinalIgnoreCase);

            if (!cleanedKeywords.All(keywordsSet.Add))
            {
                yield return new ValidationResult("Keywords list contains duplicates.", [nameof(Keywords)]);
            }
        }
    }
}

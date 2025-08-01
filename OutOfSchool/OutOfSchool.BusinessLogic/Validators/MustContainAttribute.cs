using OutOfSchool.BusinessLogic.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace OutOfSchool.BusinessLogic.Validators;
public class MustContainAttribute : ValidationAttribute
{
    private readonly RequiredCharacterType requiredCharacterType;

    private static readonly Regex LatinLetterRegex = new(@"[A-Za-z]");
    private static readonly Regex CyrillicLetterRegex = new(@"[А-Яа-я]");
    private static readonly Regex AnyLetterRegex = new(@"\p{L}");
    private static readonly Regex DigitRegex = new(@"\d");

    public MustContainAttribute(RequiredCharacterType requiredCharacterType)
    {
        this.requiredCharacterType = requiredCharacterType;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is not string str || string.IsNullOrWhiteSpace(str))
        {
            return ValidationResult.Success;
        }

        var result = requiredCharacterType switch
        {
            RequiredCharacterType.Digit => DigitRegex.IsMatch(str)
                ? ValidationResult.Success
                : new ValidationResult("Field must contain at least one digit."),
            RequiredCharacterType.LatinLetter => LatinLetterRegex.IsMatch(str)
                ? ValidationResult.Success
                : new ValidationResult("Field must contain at least one latin letter."),
            RequiredCharacterType.CyrillicLetter => CyrillicLetterRegex.IsMatch(str)
                ? ValidationResult.Success
                : new ValidationResult("Field must contain at least one cyrillic letter."),
            RequiredCharacterType.AnyLetter => AnyLetterRegex.IsMatch(str)
                ? ValidationResult.Success
                : new ValidationResult("Field must contain at least one letter."),
            _ => new ValidationResult("Invalid required character type specified.")
        };

        return result;
    }
}

using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Validators;
public class MaxDecimalPlacesAttribute : ValidationAttribute
{
    private readonly int maxDecimalPlaces;

    public MaxDecimalPlacesAttribute(int maxDecimalPlaces)
    {
        this.maxDecimalPlaces = maxDecimalPlaces;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is decimal d)
        {
            if (GetDecimalPlaces(d) > maxDecimalPlaces)
                return new ValidationResult(ErrorMessage ?? $"The field must not have more than {maxDecimalPlaces} decimal places.");
        }

        return ValidationResult.Success;
    }

    private static int GetDecimalPlaces(decimal d)
    {
        int[] bits = decimal.GetBits(d);
        return (bits[3] & 0x00FF0000) >> 16;
    }
}

using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Validators;
public class ConditionalValidationAttributes
{
    public class ConditionalRequiredAttribute : ValidationAttribute
    {
        private readonly string _featureFlagName;

        public ConditionalRequiredAttribute(string featureFlagName)
        {
            _featureFlagName = featureFlagName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var featureManager = (IFeatureManager)validationContext.GetService(typeof(IFeatureManager));

            if (featureManager == null)
            {
                return new ValidationResult("IFeatureManager service is not registered.");
            }

            bool enabled = featureManager.IsEnabledAsync(_featureFlagName).GetAwaiter().GetResult();

            if (enabled)
            {
                if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                {
                    return new ValidationResult(ErrorMessage ?? "This field is required when the feature is enabled.");
                }
            }

            return ValidationResult.Success;
        }
    }

    public class ConditionalMinLengthAttribute : ValidationAttribute
    {
        private readonly string _featureFlagName;
        private readonly int _minLength;

        public ConditionalMinLengthAttribute(string featureFlag, int minLength)
        {
            _featureFlagName = featureFlag;
            _minLength = minLength;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var featureManager = (IFeatureManager)validationContext.GetService(typeof(IFeatureManager));

            if (featureManager == null)
            {
                return new ValidationResult("IFeatureManager service is not registered.");
            }

            bool enabled = featureManager.IsEnabledAsync(_featureFlagName).GetAwaiter().GetResult();

            if (enabled)
            {
                var result = value switch
                {
                    null => new ValidationResult(ErrorMessage ?? "This field is required when the feature is enabled."),
                    string str when string.IsNullOrWhiteSpace(str) => new ValidationResult(ErrorMessage ?? "This field is required when the feature is enabled."),
                    string str when str.Length < _minLength => new ValidationResult(ErrorMessage ?? $"The field must be at least {_minLength} characters long when the feature is enabled."),
                    ICollection collection when collection.Count < _minLength => new ValidationResult(ErrorMessage ?? $"The collection must contain at least {_minLength} items when the feature is enabled."),
                    _ => ValidationResult.Success
                };

                return result;
            }

            return ValidationResult.Success;
        }
    }
}

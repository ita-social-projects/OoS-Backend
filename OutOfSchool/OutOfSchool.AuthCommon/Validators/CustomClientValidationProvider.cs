using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.Validators;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomClientValidationProvider : IValidationAttributeAdapterProvider
{
    private readonly IValidationAttributeAdapterProvider baseProvider =
        new ValidationAttributeAdapterProvider();

    public IAttributeAdapter? GetAttributeAdapter(ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
    {
        return attribute switch
        {
            CustomPasswordValidationAttribute customPasswordValidationAttribute =>
                new CustomPasswordValidationAdapter(customPasswordValidationAttribute, stringLocalizer),
            CustomUkrainianNameAttribute customUkrainianNameAttribute =>
                new CustomUkrainianNameAttributeAdapter(customUkrainianNameAttribute, stringLocalizer),
            _ => baseProvider.GetAttributeAdapter(attribute, stringLocalizer),
        };
    }
}
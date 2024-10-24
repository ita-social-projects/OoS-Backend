using System;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace OutOfSchool.Common.Validators;

public class CustomUkrainianNameAttributeAdapter : AttributeAdapterBase<CustomUkrainianNameAttribute>
{
    public CustomUkrainianNameAttributeAdapter(CustomUkrainianNameAttribute attribute, IStringLocalizer stringLocalizer)
        : base(attribute, stringLocalizer)
    {
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-regex", GetErrorMessage(context));
        MergeAttribute(context.Attributes, "data-val-regex-pattern", Constants.UkrainianNameRegexPattern);
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        return GetErrorMessage(validationContext.ModelMetadata);
    }
}

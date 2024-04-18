using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomPasswordValidationAdapter(
    CustomPasswordValidationAttribute attribute,
    IStringLocalizer? stringLocalizer)
    : AttributeAdapterBase<CustomPasswordValidationAttribute>(attribute, stringLocalizer)
{
    private readonly string validationPrefix = "data-val-validpass";

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        return GetErrorMessage(validationContext.ModelMetadata);
    }

    public override void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, validationPrefix, GetErrorMessage(context));
        MergeAttribute(context.Attributes, $"{validationPrefix}-minlength", Constants.PasswordMinLength.ToString());
        MergeAttribute(context.Attributes, $"{validationPrefix}-symbols", Constants.ValidationSymbols);
        MergeAttribute(context.Attributes, $"{validationPrefix}-upper", "true");
        MergeAttribute(context.Attributes, $"{validationPrefix}-lower", "true");
        MergeAttribute(context.Attributes, $"{validationPrefix}-number", "true");
    }
}
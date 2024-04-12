using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace OutOfSchool.AuthCommon.Validators;

public class CustomPasswordValidationAdapter : AttributeAdapterBase<CustomPasswordValidationAttribute>
{
    private readonly string validationPrefix = "data-val-validpass";
    private readonly IStringLocalizer? localizer;

    public CustomPasswordValidationAdapter(
        CustomPasswordValidationAttribute attribute,
        IStringLocalizer? stringLocalizer)
        : base(attribute, stringLocalizer)
    {
        // Property not exposed in base class, need to duplicate :(
        this.localizer = stringLocalizer;
    }

    public override string GetErrorMessage(ModelValidationContextBase validationContext)
    {
        var errorMessage = localizer?.GetString(Constants.PasswordValidationErrorMessage);
        return errorMessage ?? Constants.PasswordValidationErrorMessage;
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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutOfSchool.BusinessLogic.Util;
/// <summary>
/// Model binder for decimal values that uses a dot as the decimal separator.
/// </summary>
public class DecimalDotModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult != ValueProviderResult.None)
        {
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
            var valueAsString = valueProviderResult.FirstValue;
            
            if (!string.IsNullOrEmpty(valueAsString))
            {
                valueAsString = valueAsString.Replace(',', '.');
                
                if (decimal.TryParse(valueAsString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result))
                {
                    bindingContext.Result = ModelBindingResult.Success(result);
                }
                else
                {
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, new FormatException("Invalid decimal value format."), bindingContext.ModelMetadata);
                }
            }
        }

        return Task.CompletedTask;
    }
}
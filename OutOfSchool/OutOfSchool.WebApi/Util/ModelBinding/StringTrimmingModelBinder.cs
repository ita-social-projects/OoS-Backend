using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutOfSchool.WebApi.Util.ModelBinding;


/// <summary>
/// Model binder that trims leading and trailing whitespace from string inputs.
/// </summary>
public class StringTrimmingModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(value.Trim());
        return Task.CompletedTask;
    }
}
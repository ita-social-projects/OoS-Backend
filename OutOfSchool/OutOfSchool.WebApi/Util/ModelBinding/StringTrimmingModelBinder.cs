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
            // No value was provided -> keep existing/default property values.
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;

        if (value is null)
        {
            // Value is null -> keep existing/default property values.
            return Task.CompletedTask;
        }
        
        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
        bindingContext.ModelState.MarkFieldValid(bindingContext.ModelName);
        bindingContext.Result = ModelBindingResult.Success(value.Trim());
        return Task.CompletedTask;
    }
}
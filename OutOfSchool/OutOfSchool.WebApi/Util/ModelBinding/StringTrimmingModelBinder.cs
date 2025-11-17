using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutOfSchool.WebApi.Util.ModelBinding;

public class StringTrimmingModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
        if (valueProviderResult == ValueProviderResult.None)
            return Task.CompletedTask;

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
            return Task.CompletedTask;

        bindingContext.Result = ModelBindingResult.Success(value.Trim());
        return Task.CompletedTask;
    }
}
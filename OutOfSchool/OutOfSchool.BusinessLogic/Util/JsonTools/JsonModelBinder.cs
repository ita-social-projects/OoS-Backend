using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutOfSchool.BusinessLogic.Util.JsonTools;

public class JsonModelBinder : IModelBinder
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

            try
            {
                var result = JsonSerializerHelper.Deserialize(valueAsString, bindingContext.ModelType);
                
                if (result != null)
                {
                    bindingContext.Result = ModelBindingResult.Success(result);
                }
            }
            catch (JsonException jsonException)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, jsonException, bindingContext.ModelMetadata);
            }
        }
        return Task.CompletedTask;
    }
}
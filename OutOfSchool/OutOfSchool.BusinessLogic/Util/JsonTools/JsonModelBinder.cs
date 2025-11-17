using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

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
                var serviceProvider = bindingContext.HttpContext.RequestServices;
                var jsonOptions = serviceProvider.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;

                var result = JsonSerializerHelper.Deserialize(valueAsString, bindingContext.ModelType, jsonOptions);
                
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
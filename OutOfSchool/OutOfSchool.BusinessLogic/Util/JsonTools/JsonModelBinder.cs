using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections;

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

            // get all values
            var allValues = valueProviderResult.Values.Where(v => !string.IsNullOrEmpty(v)).ToArray();

            if (allValues.Any())
            {
                object result;

                // handle List<long>
                if (bindingContext.ModelType == typeof(List<long>))
                {
                    var longList = new List<long>();
                    foreach (var value in allValues)
                    {
                        if (long.TryParse(value, out var longValue))
                        {
                            longList.Add(longValue);
                        }
                    }
                    result = longList;
                }
                // handle List<int>
                else if (bindingContext.ModelType == typeof(List<int>))
                {
                    var intList = new List<int>();
                    foreach (var value in allValues)
                    {
                        if (int.TryParse(value, out var intValue))
                        {
                            intList.Add(intValue);
                        }
                    }
                    result = intList;
                }
                // handle complex object lists
                else if (bindingContext.ModelType.IsGenericType &&
                         bindingContext.ModelType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = bindingContext.ModelType;
                    var elementType = listType.GetGenericArguments()[0];
                    var list = (IList)Activator.CreateInstance(listType);

                    foreach (var value in allValues)
                    {
                        try
                        {
                            var deserializedItem = JsonSerializerHelper.Deserialize(value, elementType);
                            if (deserializedItem != null)
                            {
                                list.Add(deserializedItem);
                            }
                        }
                        catch
                        {
                            // Skip invalid items, continue with others
                        }
                    }
                    result = list;
                }
                
                else
                {
                    var valueAsString = valueProviderResult.FirstValue;
                    result = JsonSerializerHelper.Deserialize(valueAsString, bindingContext.ModelType);
                }

                if (result != null)
                {
                    bindingContext.Result = ModelBindingResult.Success(result);
                    return Task.CompletedTask;
                }
            }
        }
        return Task.CompletedTask;
    }
}
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace OutOfSchool.WebApi.Util.ModelBinding;

/// <summary>
/// Model binder for safely binding collections of enums from query parameters
/// </summary>
public class EnumCollectionModelBinder<T> : IModelBinder where T : struct, Enum
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var values = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (values == StringValues.Empty)
        {
            bindingContext.Result = ModelBindingResult.Success(new HashSet<T>());
            return Task.CompletedTask;
        }

        var result = new HashSet<T>();
        var invalidValues = new List<string>();

        foreach (var value in values.Values)
        {
            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (int.TryParse(value, out int intValue) && Enum.IsDefined(typeof(T), intValue))
            {
                result.Add((T)(object)intValue);
            }
            else if (Enum.TryParse<T>(value, true, out var enumValue) && Enum.IsDefined(typeof(T), enumValue))
            {
                result.Add(enumValue);
            }
            else
            {
                invalidValues.Add(value);
            }
        }

        if (invalidValues.Count != 0)
        {
            var allowedNames = string.Join(", ", Enum.GetNames<T>());
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"Invalid {typeof(T).Name} values: {string.Join(", ", invalidValues)}. " +
                $"Allowed values are: {allowedNames}.");
        }

        bindingContext.Result = ModelBindingResult.Success(result);
        return Task.CompletedTask;
    }
}

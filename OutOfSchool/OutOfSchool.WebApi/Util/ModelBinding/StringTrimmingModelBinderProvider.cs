using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutOfSchool.WebApi.Util.ModelBinding;

/// <summary>
/// Provider for registration of <see cref="StringTrimmingModelBinder"/>.
/// </summary>
public class StringTrimmingModelBinderProvider : IModelBinderProvider
{
    private static readonly StringTrimmingModelBinder Binder = new();

    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context?.Metadata?.ModelType == null)
            return null;

        var modelType = context.Metadata.ModelType;

        if (modelType == typeof(string) && context.BindingInfo?.BindingSource != BindingSource.Body)
        {
            return Binder;
        }

        return null;
    }
}
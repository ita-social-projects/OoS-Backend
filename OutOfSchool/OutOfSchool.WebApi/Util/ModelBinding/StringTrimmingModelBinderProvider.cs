using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutOfSchool.WebApi.Util.ModelBinding;

/// <summary>
/// Provider for registration of <see cref="StringTrimmingModelBinder"/>.
/// </summary>
public class StringTrimmingModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context?.Metadata?.ModelType == null)
            return null;

        var modelType = context.Metadata.ModelType;

        if (modelType == typeof(string) && context.BindingInfo?.BindingSource != BindingSource.Body)
        {
            return new StringTrimmingModelBinder();
        }

        return null;
    }
}
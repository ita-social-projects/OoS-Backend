using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OutOfSchool.WebApi.Util.ModelBinding;

/// <summary>
/// Provider for automatic registration of enum collection model binders
/// </summary>
public class EnumCollectionModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context?.Metadata?.ModelType == null)
            return null;

        var modelType = context.Metadata.ModelType;

        if (IsEnumHashSet(modelType))
        {
            var enumType = modelType.GetGenericArguments()[0];
            var binderType = typeof(EnumCollectionModelBinder<>).MakeGenericType(enumType);

            return (IModelBinder)Activator.CreateInstance(binderType);
        }

        return null;
    }

    private static bool IsEnumHashSet(Type modelType)
    {
        return modelType.IsGenericType &&
               modelType.GetGenericTypeDefinition() == typeof(HashSet<>) &&
               modelType.GetGenericArguments().Length == 1 &&
               modelType.GetGenericArguments()[0].IsEnum;
    }
}

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace OutOfSchool.Tests.Common;
public static class TestModelBinderHelpers
{
    // Helper methods
    public static ModelMetadata CreateModelMetadata(Type modelType)
    {
        if (modelType == null)
            return null;

        var metadataProvider = new EmptyModelMetadataProvider();
        return metadataProvider.GetMetadataForType(modelType);
    }

    public static ModelBinderProviderContext CreateContext(ModelMetadata metadata)
    {
        var context = new TestModelBinderProviderContext();
        context.SetMetadata(metadata);
        return context;
    }

    // Custom implementation of ModelBinderProviderContext for testing
    public class TestModelBinderProviderContext : ModelBinderProviderContext
    {
        private ModelMetadata _metadata;
        private BindingInfo _bindingInfo = new();

        public override ModelMetadata Metadata => _metadata;

        public void SetMetadata(ModelMetadata metadata)
        {
            _metadata = metadata;
        }

        public override BindingInfo BindingInfo => _bindingInfo;

        public override IModelBinder CreateBinder(ModelMetadata metadata)
        {
            throw new NotImplementedException("Not needed for these tests");
        }

        public override IModelMetadataProvider MetadataProvider => null;
    }
}
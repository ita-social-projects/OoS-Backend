using System;

namespace OutOfSchool.WebApi.Common.Resources
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ResourcesKeyAttribute : Attribute
    {
        public ResourcesKeyAttribute(string appResourcesKey) => ResourcesKey = appResourcesKey;

        public string ResourcesKey { get; set; }
    }
}

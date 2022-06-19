using System;
using System.Collections.Generic;
using Nest;

namespace OutOfSchool.WebApi.Util.Elasticsearch
{
    public class IcuCollationKeywordProperty : IProperty
    {
        public IcuCollationKeywordProperty(string language, string country)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            Country = country ?? throw new ArgumentNullException(nameof(country));
        }

        IDictionary<string, object> IProperty.LocalMetadata { get; set; }

        IDictionary<string, string> IProperty.Meta { get; set; }

        public PropertyName Name { get; set; } = PropertyConstants.DefaultFieldsKeywordName;

        public string Type { get; set; } = PropertyConstants.IcuCollationKeywordTypeName;

        [PropertyName("language")]
        public string Language { get; }

        [PropertyName("country")]
        public string Country { get; }
    }
}
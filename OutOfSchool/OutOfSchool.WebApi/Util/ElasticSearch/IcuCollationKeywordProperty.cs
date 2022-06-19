using System;
using System.Collections.Generic;
using Nest;

namespace OutOfSchool.WebApi.Util.ElasticSearch
{
    public class IcuCollationKeywordProperty : IProperty
    {
        public IcuCollationKeywordProperty(string name, string language, string country)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Language = language ?? throw new ArgumentNullException(nameof(language));
            Country = country ?? throw new ArgumentNullException(nameof(country));
        }

        IDictionary<string, object> IProperty.LocalMetadata { get; set; }

        IDictionary<string, string> IProperty.Meta { get; set; }

        public PropertyName Name { get; set; }

        public string Type { get; set; } = "icu_collation_keyword";

        [PropertyName("language")]
        public string Language { get; }

        [PropertyName("country")]
        public string Country { get; }
    }
}
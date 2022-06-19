using System;
using Nest;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Util.Elasticsearch;

namespace OutOfSchool.WebApi.Config.Elasticsearch
{
    public class ElasticsearchWorkshopConfiguration : IElasticsearchEntityTypeConfiguration
    {
        private const string DefaultLanguage = "uk";
        private const string DefaultCountry = "UA";

        public ICreateIndexRequest Configure(CreateIndexDescriptor indexDescriptor)
        {
            _ = indexDescriptor ?? throw new ArgumentNullException(nameof(indexDescriptor));

            indexDescriptor
                .Map<WorkshopES>(x => x.AutoMap<WorkshopES>()
                    .Properties(ps => ps
                        .Text(t => t
                            .Name(n => n.Title)
                            .Fields(f =>
                                f.Custom(new IcuCollationKeywordProperty("keyword", DefaultLanguage, DefaultCountry))))));

            return indexDescriptor;
        }
    }
}
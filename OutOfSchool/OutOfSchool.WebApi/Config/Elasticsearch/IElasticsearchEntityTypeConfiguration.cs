using System;
using Nest;

namespace OutOfSchool.WebApi.Config.Elasticsearch
{
    public interface IElasticsearchEntityTypeConfiguration
    {
        ICreateIndexRequest Configure(CreateIndexDescriptor indexDescriptor);
    }
}
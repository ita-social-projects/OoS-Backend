using Nest;
using OutOfSchool.WebApi.Util.Elasticsearch;

namespace OutOfSchool.WebApi.Config.Elasticsearch;

/// <summary>
/// Contains methods to configure <see cref="WorkshopES"/> model into Elasticsearch index.
/// </summary>
public class ElasticsearchWorkshopConfiguration : IElasticsearchEntityTypeConfiguration
{
    private const string DefaultLanguage = "uk";
    private const string DefaultCountry = "UA";

    /// <inheritdoc/>
    public ICreateIndexRequest Configure(CreateIndexDescriptor indexDescriptor)
    {
        _ = indexDescriptor ?? throw new ArgumentNullException(nameof(indexDescriptor));

        indexDescriptor
            .Map<WorkshopES>(x => x.AutoMap<WorkshopES>()
                .Properties(ps => ps
                    .Text(t => t
                        .Name(n => n.Title)
                        .Fields(f =>
                            f.Custom(new IcuCollationKeywordProperty(DefaultLanguage, DefaultCountry))))));

        return indexDescriptor;
    }
}
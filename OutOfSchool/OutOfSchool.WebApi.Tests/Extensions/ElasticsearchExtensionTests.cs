using System.Collections.Generic;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class ElasticsearchExtensionTests
{
    [Test]
    public void ElasticsearchClient_ServiceRegistration()
    {
        // Arrange
        var mockConfig = new ElasticConfig();
        mockConfig.Urls = new List<string>();
        mockConfig.Urls.Add("https://url");
        mockConfig.User = "user";
        mockConfig.Password = "password";
        var coll = new ServiceCollection();

        // Act
        coll.AddElasticsearch(mockConfig);
        ServiceProvider provider = coll.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOf<ElasticsearchClient>(provider.GetService<ElasticsearchClient>());
    }
}
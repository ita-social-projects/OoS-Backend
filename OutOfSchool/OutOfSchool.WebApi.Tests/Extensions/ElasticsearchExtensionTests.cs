using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nest;
using NUnit.Framework;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions
{
    [TestFixture]
    public class ElasticsearchExtensionTests
    {
        [Test]
        public void ElasticsearchClient_ServiceRegistration()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x["elasticsearch:url"]).Returns("https://url");
            mockConfig.Setup(x => x["elasticsearch:user"]).Returns("user");
            mockConfig.Setup(x => x["elasticsearch:password"]).Returns("password");
            var coll = new ServiceCollection();

            // Act
            coll.AddElasticsearch(mockConfig.Object);
            ServiceProvider provider = coll.BuildServiceProvider();

            // Assert
            Assert.IsInstanceOf<ElasticClient>(provider.GetService<ElasticClient>());
        }
    }
}

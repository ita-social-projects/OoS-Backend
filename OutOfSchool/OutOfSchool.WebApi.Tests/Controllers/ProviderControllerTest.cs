using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.Tests
{
    [TestFixture]
#pragma warning disable SA1649 // File name should match first type name
    public class Tests
#pragma warning restore SA1649 // File name should match first type name
    {
        private readonly ILogger<ProviderController> logger;
        private IProviderService providerService;

        public Tests()
        {
            logger = new Mock<ILogger<ProviderController>>().Object;
            providerService = new Mock<IProviderService>().Object;
        }

        public ProviderController CreateProviderController => new ProviderController(logger, providerService);

        [SetUp]
        public void Setup()
        {
        }
    }
}
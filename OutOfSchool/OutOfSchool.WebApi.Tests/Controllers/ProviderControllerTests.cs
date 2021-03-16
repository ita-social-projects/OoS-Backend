using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.Tests
{
    [TestFixture]
    public class ProviderControllerTests
    {
        private readonly ILogger<ProviderController> logger;
        private IProviderService providerService;

        public ProviderControllerTests()
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
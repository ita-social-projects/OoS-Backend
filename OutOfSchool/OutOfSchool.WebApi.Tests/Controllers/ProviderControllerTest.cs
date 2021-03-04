using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.Tests
{
    [TestFixture]
    public class Tests
    {
        private readonly ILogger<ProviderController> logger;
        private IProviderService providerService;
        public ProviderController CreateProviderController => new ProviderController(logger, providerService);

        public Tests()
        {
            logger = new Mock<ILogger<ProviderController>>().Object;
            providerService = new Mock<IProviderService>().Object;
        }
        [SetUp]
        public void Setup()
        {
        }
    }
}
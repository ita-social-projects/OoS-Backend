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
        private readonly ILogger<ProviderController> _logger;
        private IProviderService organizationService;
        public ProviderController CreateOrganizationController => new ProviderController(_logger, organizationService);

        public Tests()
        {
            _logger = new Mock<ILogger<ProviderController>>().Object;
            organizationService = new Mock<IProviderService>().Object;
        }
        [SetUp]
        public void Setup()
        {
        }
    }
}
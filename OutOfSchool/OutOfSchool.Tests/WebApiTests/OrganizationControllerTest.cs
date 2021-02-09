using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Services.Interfaces;

namespace OutOfSchool.Tests.WebApiTests
{
    [TestFixture]
    public class Tests
    {
        private readonly ILogger<OrganizationController> _logger;
        private IOrganizationService organizationService;
        public OrganizationController CreateOrganizationController => new OrganizationController(_logger, organizationService);

        public Tests()
        {
            _logger = new Mock<ILogger<OrganizationController>>().Object;
            organizationService = new Mock<IOrganizationService>().Object;
        }
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            CreateOrganizationController.TestOk();
            Assert.Pass();
        }
    }
}
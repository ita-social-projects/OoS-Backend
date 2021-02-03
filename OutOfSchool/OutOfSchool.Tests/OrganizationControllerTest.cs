using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;

namespace OutOfSchool.Tests
{
    [TestFixture]
    public class Tests
    {
        private readonly ILogger<OrganizationController> _logger;
        public OrganizationController CreateOrganizationController => new OrganizationController(_logger);

        public Tests()
        {
            _logger = new Mock<ILogger<OrganizationController>>().Object;
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
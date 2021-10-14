using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class InstitutionStatusControllerTests
    {
        private InstitutionStatusController controller;
        private Mock<IStatusService> service;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<InstitutionStatusDTO> institutionStatuses;
        private InstitutionStatusDTO institutionStatus;



        [SetUp]
        public void Setup()
        {
            service = new Mock<IStatusService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new InstitutionStatusController(service.Object, localizer.Object);

            institutionStatuses = FakeInstitutionStatuses();
            institutionStatus = FakeInstitutionStatus();
        }

        [Test]
        public async Task GetInstitutionStatuses_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(institutionStatuses);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        private InstitutionStatusDTO FakeInstitutionStatus()
        {
            return new InstitutionStatusDTO()
            {
                Id = 1,
                Name = "Test",
            };
        }

        private IEnumerable<InstitutionStatusDTO> FakeInstitutionStatuses()
        {
            return new List<InstitutionStatusDTO>()
            {
                new InstitutionStatusDTO()
                {
                Id = 1,
                Name = "NoName",
                },
                new InstitutionStatusDTO()
                {
                Id = 2,
                Name = "HaveName",
                },
                new InstitutionStatusDTO()
                {
                Id = 3,
                Name = "MissName",
                },
            };
        }
    }
}

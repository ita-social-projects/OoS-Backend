using System;
using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public async Task GetInstitutionStatuses_WhenEmptyCollection_ReturnsNoContentResult()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(new List<InstitutionStatusDTO>());

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetInstitutionStatusById_WhenIdIsValid_ReturnOkResultObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(institutionStatuses.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(-50)]
        public void GetInstitutionStatusById_WhenIdIsInvalid_ReturnsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(institutionStatuses.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }


        [Test]
        [TestCase(100)]
        public async Task GetInstitutionStatusById_WhenIdIsNotValid_ReturnsEmptyObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(institutionStatuses.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetInstitutionStatus_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(institutionStatus)).ReturnsAsync(institutionStatus);

            // Act
            var result = await controller.Create(institutionStatus).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }




        [Test]
        public async Task UpdateInstitutionStatus_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            service.Setup(x => x.Update(institutionStatus)).ReturnsAsync(institutionStatus);

            // Act
            var result = await controller.Update(institutionStatus).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }


        [Test]
        [TestCase(1)]
        public async Task DeleteInstitutionStatus_WhenIdIsValid_ReturnsNoContentResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(-50)]
        public void DeleteInstitutionStatus_WhenIdIsInvalid_ReturnsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteInstitutionStatus_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }


        /// <summary>
        /// faking data for testing.
        /// </summary>
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

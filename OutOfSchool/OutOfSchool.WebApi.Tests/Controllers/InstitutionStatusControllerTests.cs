using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Tests.Common;
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
            var response = await controller.Get().ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseOkAndValidValue<IEnumerable<InstitutionStatusDTO>>();
        }

        [Test]
        public async Task GetInstitutionStatuses_WhenEmptyCollection_ReturnsNoContentResult()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(new List<InstitutionStatusDTO>());

            // Act
            var response = await controller.Get().ConfigureAwait(false);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(response);
        }

        [Test]
        [TestCase(1)]
        public async Task GetInstitutionStatusById_WhenIdIsValid_ReturnOkResultObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(institutionStatuses.SingleOrDefault(x => x.Id == id));

            // Act
            var response = await controller.GetById(id).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseOkAndValidValue<InstitutionStatusDTO>();
        }

        [Test]
        [TestCase(-50)]
        public async Task GetInstitutionStatusById_WhenIdIsInvalid_ReturnsBadRequestWithExceptionMessage(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(institutionStatuses.SingleOrDefault(x => x.Id == id));

            // Act
            var response = await controller.GetById(id).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        [TestCase(100)]
        public async Task GetById_WhenIdDoesntExist_ReturnsBadRequestWithExceptionMessage(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).Throws<ArgumentOutOfRangeException>();

            // Act
            var response = await controller.GetById(id).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        public async Task CreateInstitutionStatus_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(institutionStatus)).ReturnsAsync(institutionStatus);

            // Act
            var response = await controller.Create(institutionStatus).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseValidateValueNotEmpty<CreatedAtActionResult>();
        }

        [Test]
        public async Task UpdateInstitutionStatus_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            service.Setup(x => x.Update(institutionStatus)).ReturnsAsync(institutionStatus);

            // Act
            var response = await controller.Update(institutionStatus).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseOkAndValidValue<InstitutionStatusDTO>();
        }


        [Test]
        [TestCase(1)]
        public async Task DeleteInstitutionStatus_WhenIdIsValid_ReturnsNoContentResult(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var response = await controller.Delete(id);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(response);
        }

        [Test]
        [TestCase(-50)]
        public async Task Delete_WhenIdIsInvalid_ReturnsBadRequestWithExceptionMessageAsync(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var response = await controller.Delete(id).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteInstitutionStatus_WhenIdDoesntExists_ReturnsBadRequestWithMessage(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id)).Throws<ArgumentOutOfRangeException>();

            // Act
            var response = await controller.Delete(id).ConfigureAwait(false);

            // Assert
            response.GetAssertedResponseValidateValueNotEmpty<BadRequestObjectResult>();
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

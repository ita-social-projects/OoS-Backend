using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Extensions;
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

        private IEnumerable<InstitutionStatus> institutionStatuses;
        private InstitutionStatus institutionStatus;



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
            var expected = institutionStatuses.Select(x => x.ToModel());
            service.Setup(x => x.GetAll()).ReturnsAsync(institutionStatuses.Select(x => x.ToModel()));

            // Act
            var response = await controller.Get().ConfigureAwait(false);

            // Assert
            response.AssertResponseOkResultAndValidateValue(expected);
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
            var expected = institutionStatuses.SingleOrDefault(x => x.Id == id).ToModel();
            service.Setup(x => x.GetById(id)).ReturnsAsync(institutionStatuses.SingleOrDefault(x => x.Id == id).ToModel());

            // Act
            var response = await controller.GetById(id).ConfigureAwait(false);

            // Assert
            response.AssertResponseOkResultAndValidateValue(expected);
        }

        [Test]
        [TestCase(-50)]
        public async Task GetInstitutionStatusById_WhenIdIsInvalid_ReturnsBadRequestWithExceptionMessage(long id)
        {
            // Arrange

            var exceptedResponse = new BadRequestObjectResult(TestDataHelper.GetRandomWords());
            service.Setup(x => x.GetById(id)).ReturnsAsync(institutionStatuses.SingleOrDefault(x => x.Id == id).ToModel());

            // Act
            var response = await controller.GetById(id).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(exceptedResponse);
        }

        [Test]
        [TestCase(100)]
        public async Task GetById_WhenIdDoesntExist_ReturnsBadRequestWithExceptionMessage(long id)
        {
            // Arrange
            var expectedResponse = new BadRequestObjectResult(TestDataHelper.GetRandomWords());
            service.Setup(x => x.GetById(id)).Throws<ArgumentOutOfRangeException>();

            // Act
            var response = await controller.GetById(id).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expectedResponse);
        }

        [Test]
        public async Task CreateInstitutionStatus_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var expected = institutionStatus.ToModel();
            var expectedResponse = new CreatedAtActionResult(nameof(controller.GetById), nameof(controller), new { id = expected.Id }, expected);
            service.Setup(x => x.Create(expected)).ReturnsAsync(institutionStatus.ToModel());

            // Act
            var response = await controller.Create(expected).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<CreatedAtActionResult>(expectedResponse);
        }

        [Test]
        public async Task UpdateInstitutionStatus_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            var expected = institutionStatus.ToModel();
            service.Setup(x => x.Update(expected)).ReturnsAsync(institutionStatus.ToModel());

            // Act
            var response = await controller.Update(expected).ConfigureAwait(false);

            // Assert
            response.AssertResponseOkResultAndValidateValue(expected);
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
            var expected = new BadRequestObjectResult(TestDataHelper.GetRandomWords());
            service.Setup(x => x.Delete(id));

            // Act
            var response = await controller.Delete(id).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteInstitutionStatus_WhenIdDoesntExists_ReturnsBadRequestWithMessage(long id)
        {
            // Arrange
            var expected = new BadRequestObjectResult(TestDataHelper.GetRandomWords());
            service.Setup(x => x.Delete(id)).Throws<ArgumentOutOfRangeException>();

            // Act
            var response = await controller.Delete(id).ConfigureAwait(false);

            // Assert
            response.AssertExpectedResponseTypeAndCheckDataInside<BadRequestObjectResult>(expected);
        }


        /// <summary>
        /// faking data for testing.
        /// </summary>
        private InstitutionStatus FakeInstitutionStatus()
        {
            return new InstitutionStatus()
            {
                Id = 1,
                Name = TestDataHelper.GetRandomWords(),
            };
        }

        private IEnumerable<InstitutionStatus> FakeInstitutionStatuses()
        {
            return new List<InstitutionStatus>()
            {
                new InstitutionStatus()
                {
                Id = 1,
                Name = TestDataHelper.GetRandomWords(),
                },
                new InstitutionStatus()
                {
                Id = 2,
                Name = TestDataHelper.GetRandomWords(),
                },
                new InstitutionStatus()
                {
                Id = 3,
                Name = TestDataHelper.GetRandomWords(),
                },
            };
        }
    }
}

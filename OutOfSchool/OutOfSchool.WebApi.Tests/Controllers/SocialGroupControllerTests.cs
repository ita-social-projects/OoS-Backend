using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SocialGroupControllerTests
    {
        private SocialGroupController controller;
        private Mock<ISocialGroupService> service;
        private Mock<IStringLocalizer<SharedResource>> localizer;

        private IEnumerable<SocialGroupDto> socialGroups;
        private SocialGroupDto socialGroup;

        [SetUp]
        public void Setup()
        {
            service = new Mock<ISocialGroupService>();
            localizer = new Mock<IStringLocalizer<SharedResource>>();

            controller = new SocialGroupController(service.Object, localizer.Object);

            socialGroups = FakeSocialGroups();
            socialGroup = FakeSocialGroup();
        }

        [Test]
        public async Task GetSocialGroups_WhenCalled_ReturnsOkResultObject()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(socialGroups);

            // Act
            var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task GetSocialGroups_WhenEmptyCollection_ReturnsNoContentResult()
        {
            // Arrange
            service.Setup(x => x.GetAll()).ReturnsAsync(new List<SocialGroupDto>());

            // Act
            var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(204, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task GetSocialGroupById_WhenIdIsValid_ReturnOkResultObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(socialGroups.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(-50)]
        public void GetSocialGroupById_WhenIdIsInvalid_ReturnsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(socialGroups.SingleOrDefault(x => x.Id == id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.GetById(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(100)]
        public async Task GetSocialGroupById_WhenIdIsNotValid_ReturnsEmptyObject(long id)
        {
            // Arrange
            service.Setup(x => x.GetById(id)).ReturnsAsync(socialGroups.SingleOrDefault(x => x.Id == id));

            // Act
            var result = await controller.GetById(id).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        public async Task CreateSocialGroup_WhenModelIsValid_ReturnsCreatedAtActionResult()
        {
            // Arrange
            service.Setup(x => x.Create(socialGroup)).ReturnsAsync(socialGroup);

            // Act
            var result = await controller.Create(socialGroup).ConfigureAwait(false) as CreatedAtActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(201, result.StatusCode);
        }

        [Test]
        public async Task UpdateSocialGroup_WhenModelIsValid_ReturnsOkObjectResult()
        {
            // Arrange
            service.Setup(x => x.Update(socialGroup)).ReturnsAsync(socialGroup);

            // Act
            var result = await controller.Update(socialGroup).ConfigureAwait(false) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Test]
        [TestCase(1)]
        public async Task DeleteSocialGroup_WhenIdIsValid_ReturnsNoContentResult(long id)
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
        public void DeleteSocialGroup_WhenIdIsInvalid_ReturnsArgumentOutOfRangeException(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act and Assert
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await controller.Delete(id).ConfigureAwait(false));
        }

        [Test]
        [TestCase(10)]
        public async Task DeleteSocialGroup_WhenIdIsInvalid_ReturnsNull(long id)
        {
            // Arrange
            service.Setup(x => x.Delete(id));

            // Act
            var result = await controller.Delete(id) as OkObjectResult;

            // Assert
            Assert.That(result, Is.Null);
        }

        private SocialGroupDto FakeSocialGroup()
        {
            return new SocialGroupDto()
            {
                Id = 1,
                Name = "Test",
            };
        }

        private IEnumerable<SocialGroupDto> FakeSocialGroups()
        {
            return new List<SocialGroupDto>()
            {
                new SocialGroupDto()
                {
                Id = 1,
                Name = "NoName",
                },
                new SocialGroupDto()
                {
                Id = 2,
                Name = "HaveName",
                },
                new SocialGroupDto()
                {
                Id = 3,
                Name = "MissName",
                },
            };
        }
    }
}

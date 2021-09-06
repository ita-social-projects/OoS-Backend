using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class ValidationServiceTests
    {
        private Mock<IProviderRepository> providerRepositoryMock;
        private Mock<IParentRepository> parentRepositoryMock;
        private Mock<IWorkshopRepository> workshopRepositoryMock;

        private IValidationService validationService;

        [SetUp]
        public void SetUp()
        {
            providerRepositoryMock = new Mock<IProviderRepository>();
            parentRepositoryMock = new Mock<IParentRepository>();
            workshopRepositoryMock = new Mock<IWorkshopRepository>();

            validationService = new ValidationService(providerRepositoryMock.Object, parentRepositoryMock.Object, workshopRepositoryMock.Object);
        }

        #region UserIsProviderOwner
        [Test]
        public async Task UserIsProviderOwnerAsync_WhenTrue_ReturnsTrue()
        {
            // Arrange
            var validUserId = "someUserId";
            var providerWithValidUserId = new Provider()
            {
                Id = 1,
                UserId = validUserId,
            };
            providerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Provider, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Provider>() { providerWithValidUserId });

            // Act
            var result = await validationService.UserIsProviderOwnerAsync(validUserId, providerWithValidUserId.Id).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task UserIsProviderOwnerAsync_WhenFalse_ReturnsFalse()
        {
            // Arrange
            var validUserId = "someUserId";
            var providerWithAnotherUserId = new Provider()
            {
                Id = 1,
                UserId = "anotherUserId",
            };
            providerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Provider, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Provider>() { providerWithAnotherUserId });

            // Act
            var result = await validationService.UserIsProviderOwnerAsync(validUserId, providerWithAnotherUserId.Id).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UserIsProviderOwnerAsync_WhenEntityWasNotFaund_ReturnsFalse()
        {
            // Arrange
            var validUserId = "someUserId";
            providerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Provider, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Provider>() { });

            // Act
            var result = await validationService.UserIsProviderOwnerAsync(validUserId, 1).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region UserIsWorkshopOwner
        [Test]
        public async Task UserIsWorkshopOwnerAsync_WhenTrue_ReturnsTrue()
        {
            // Arrange
            var validUserId = "someUserId";
            var workshopWithProviderWithValidUserId = new Workshop()
            {
                Id = 1,
                Provider = new Provider()
                {
                    Id = 1,
                    UserId = validUserId,
                },
            };
            workshopRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Workshop>() { workshopWithProviderWithValidUserId });

            // Act
            var result = await validationService.UserIsWorkshopOwnerAsync(validUserId, workshopWithProviderWithValidUserId.Id).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task UserIsWorkshopOwnerAsync_WhenFalse_ReturnsFalse()
        {
            // Arrange
            var validUserId = "someUserId";
            var workshopWithProviderWithAnotherUserId = new Workshop()
            {
                Id = 1,
                Provider = new Provider()
                {
                    Id = 1,
                    UserId = "anotherUserId",
                },
            };
            workshopRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Workshop>() { workshopWithProviderWithAnotherUserId });

            // Act
            var result = await validationService.UserIsWorkshopOwnerAsync(validUserId, workshopWithProviderWithAnotherUserId.Id).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UserIsWorkshopOwnerAsync_WhenEntityWasNotFaund_ReturnsFalse()
        {
            // Arrange
            var validUserId = "someUserId";
            workshopRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Workshop, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Workshop>());

            // Act
            var result = await validationService.UserIsWorkshopOwnerAsync(validUserId, 1).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region UserIsParentOwnerAsync
        [Test]
        public async Task UserIsParentOwnerAsync_WhenTrue_ReturnsTrue()
        {
            // Arrange
            var validUserId = "someUserId";
            var parentWithValidUserId = new Parent()
            {
                Id = 1,
                UserId = validUserId,
            };
            parentRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Parent, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Parent>() { parentWithValidUserId });

            // Act
            var result = await validationService.UserIsParentOwnerAsync(validUserId, parentWithValidUserId.Id).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task UserIsParentOwnerAsync_WhenFalse_ReturnsFalse()
        {
            // Arrange
            var validUserId = "someUserId";
            var parentWithAnotherUserId = new Parent()
            {
                Id = 1,
                UserId = "anotherUserId",
            };
            parentRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Parent, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Parent>() { parentWithAnotherUserId });

            // Act
            var result = await validationService.UserIsParentOwnerAsync(validUserId, parentWithAnotherUserId.Id).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task UserIsParentOwnerAsync_WhenEntityWasNotFaund_ReturnsFalse()
        {
            // Arrange
            var validUserId = "someUserId";
            parentRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Parent, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Parent>());

            // Act
            var result = await validationService.UserIsParentOwnerAsync(validUserId, 1).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetParentOrProviderIdByUserRoleAsync
        [Test]
        public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsParentAndParentExists_ReturnsId()
        {
            // Arrange
            var validUserId = "someUserId";
            var userRole = Role.Parent;

            var parentWithValidUserId = new Parent()
            {
                Id = 1,
                UserId = validUserId,
            };
            parentRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Parent, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Parent>() { parentWithValidUserId });

            // Act
            var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(parentWithValidUserId.Id, result);
        }

        [Test]
        public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsParentAndParentDoesNotExist_ReturnsZero()
        {
            // Arrange
            var validUserId = "someUserId";
            var userRole = Role.Parent;

            parentRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Parent, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Parent>() { });

            // Act
            var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(default(long), result);
        }

        [Test]
        public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsProviderAndProviderExists_ReturnsId()
        {
            // Arrange
            var validUserId = "someUserId";
            var userRole = Role.Provider;

            var providerWithValidUserId = new Provider()
            {
                Id = 1,
                UserId = validUserId,
            };
            providerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Provider, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Provider>() { providerWithValidUserId });

            // Act
            var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(providerWithValidUserId.Id, result);
        }

        [Test]
        public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsProviderAndProviderDoesNotExist_ReturnsZero()
        {
            // Arrange
            var validUserId = "someUserId";
            var userRole = Role.Provider;

            providerRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Provider, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(new List<Provider>());

            // Act
            var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(default(long), result);
        }

        [Test]
        public async Task GetParentOrProviderIdByUserRoleAsync_RoleIsNotParentNotProvider_ReturnsZero()
        {
            // Arrange
            var validUserId = "someUserId";
            var userRole = Role.Admin;

            // Act
            var result = await validationService.GetParentOrProviderIdByUserRoleAsync(validUserId, userRole).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(default(long), result);
        }
        #endregion
    }
}

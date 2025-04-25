using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Parent;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]

public class ParentServiceTests
{
    private IParentService parentService;
    private Mock<IParentRepository> parentRepositoryMock;
    private Mock<ICurrentUserService> currentUserServiceMock;
    private Mock<IParentBlockedByAdminLogService> parentBlockedByAdminLogServiceMock;
    private Mock<ILogger<ParentService>> loggerMock;
    private Mock<IEntityRepositorySoftDeleted<Guid, Child>> repositoryChildMock;
    private Mock<IUserService> userService;
    private Mock<IEntityRepositorySoftDeleted<string, User>> userRepositoryMock;
    private Faker faker;

    [SetUp]
    public void SetUp()
    {
        parentRepositoryMock = new Mock<IParentRepository>();
        currentUserServiceMock = new Mock<ICurrentUserService>();
        parentBlockedByAdminLogServiceMock = new Mock<IParentBlockedByAdminLogService>();
        loggerMock = new Mock<ILogger<ParentService>>();
        repositoryChildMock = new Mock<IEntityRepositorySoftDeleted<Guid, Child>>();
        userService = new Mock<IUserService>();
        userRepositoryMock = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        faker = new();

        parentService = new ParentService(
            parentRepositoryMock.Object,
            currentUserServiceMock.Object,
            parentBlockedByAdminLogServiceMock.Object,
            loggerMock.Object,
            repositoryChildMock.Object,
            userService.Object,
            userRepositoryMock.Object);
    }

    #region Create
    [Test]
    public void Create_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await parentService.Create(null).ConfigureAwait(false));
    }

    [TestCase(null)]
    [TestCase("")]
    public void Create_WhenUserIdIsInvalid_ShouldLogErrorAndThrowInvalidOperationException(string userId)
    {
        // Arrange
        currentUserServiceMock.SetupGet(s => s.UserId).Returns(userId);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await parentService.Create(new ParentCreateDto()).ConfigureAwait(false));

        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                0,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public void Create_WhenUserNotExists_ShouldLogErrorAndThrowInvalidOperationException()
    {
        // Arrange
        currentUserServiceMock
            .SetupGet(s => s.UserId)
            .Returns("userId")
            .Verifiable(Times.Once);

        userRepositoryMock
            .Setup(r => r.GetById("userId"))
            .ReturnsAsync(null as User)
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await parentService.Create(new ParentCreateDto()).ConfigureAwait(false));

        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                0,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        Mock.VerifyAll();
    }

    [Test]
    public void Create_WhenParentExists_ShouldLogErrorAndThrowInvalidOperationException()
    {
        // Arrange
        currentUserServiceMock.SetupGet(s => s.UserId).Returns("userId");

        userRepositoryMock
            .Setup(r => r.GetById("userId"))
            .ReturnsAsync(new User())
            .Verifiable(Times.Once);

        parentRepositoryMock
            .Setup(r => r.Any(It.IsAny<Expression<Func<Parent, bool>>>()))
            .ReturnsAsync(true)
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await parentService.Create(new ParentCreateDto()).ConfigureAwait(false));

        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                0,
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        Mock.VerifyAll();
    }

    [Test]
    public async Task Create_WhenUserExists_ShouldSetUserIsRegistered()
    {
        // Arrange
        var user = UserGenerator.Generate();

        currentUserServiceMock.SetupGet(s => s.UserId).Returns(user.Id);

        userRepositoryMock
            .Setup(r => r.GetById(user.Id))
            .ReturnsAsync(user)
            .Verifiable(Times.Once);

        parentRepositoryMock
            .Setup(r => r.Create(It.IsAny<Parent>()))
            .ReturnsAsync(new Parent())
            .Verifiable(Times.Once);

        parentRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Parent>>>()))
            .Returns((Func<Task<Parent>> f) => f.Invoke())
            .Verifiable(Times.Once);

        // Act
        await parentService.Create(new ParentCreateDto()).ConfigureAwait(false);

        // Assert
        Assert.True(user.IsRegistered);
        Mock.VerifyAll();
    }

    [Test]
    public async Task Create_WhenUserExists_ShouldSetUserPhoneNumber()
    {
        // Arrange
        var user = UserGenerator.Generate();
        var expectedPhoneNumber = faker.Phone.PhoneNumber("+############");

        currentUserServiceMock.SetupGet(s => s.UserId).Returns(user.Id);

        userRepositoryMock
            .Setup(r => r.GetById(user.Id))
            .ReturnsAsync(user)
            .Verifiable(Times.Once);

        parentRepositoryMock
            .Setup(r => r.Create(It.IsAny<Parent>()))
            .ReturnsAsync(new Parent())
            .Verifiable(Times.Once);

        parentRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Parent>>>()))
            .Returns((Func<Task<Parent>> f) => f.Invoke())
            .Verifiable(Times.Once);

        // Act
        await parentService.Create(new ParentCreateDto() { PhoneNumber = expectedPhoneNumber }).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expectedPhoneNumber, user.PhoneNumber);
        Mock.VerifyAll();
    }

    [Test]
    public async Task Create_WhenUserExists_ShouldReturnValidIds()
    {
        // Arrange
        var user = UserGenerator.Generate();
        var parent = ParentGenerator.Generate();

        currentUserServiceMock.SetupGet(s => s.UserId).Returns(user.Id);

        userRepositoryMock
            .Setup(r => r.GetById(user.Id))
            .ReturnsAsync(user)
            .Verifiable(Times.Once);

        parentRepositoryMock
            .Setup(r => r.Create(It.IsAny<Parent>()))
            .ReturnsAsync(parent)
            .Verifiable(Times.Once);

        parentRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task<Parent>>>()))
            .Returns((Func<Task<Parent>> f) => f.Invoke())
            .Verifiable(Times.Once);

        // Act
        var result = await parentService.Create(new ParentCreateDto()).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(parent.Id, result.Id);
        Assert.AreEqual(parent.UserId, result.UserId);
        Mock.VerifyAll();
    }
    #endregion

    #region BlockUblockParent
    [Test]
    public async Task BlockUnblockParent_WhenBlockUnblockParentDtoIsValid_ShouldReturnSuccess()
    {
        // Arrange
        var expected = Result<bool>.Success(true);

        BlockUnblockParentDto parentBlockUnblockValid = new()
        {
            ParentId = Guid.NewGuid(),
            IsBlocked = true,
            Reason = "Reason to block the parent",
        };

        User parentUser = UserGenerator.Generate();
        parentUser.IsBlocked = false;
        Parent parent = new()
        {
            Id = parentBlockUnblockValid.ParentId,
            UserId = parentUser.Id,
            User = parentUser,
            IsDeleted = false,
        };
        var resultOfSavingToDb = 1;
        parentRepositoryMock
            .Setup(x => x.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parent)
            .Verifiable(Times.Once);
        parentRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultOfSavingToDb)
            .Verifiable(Times.Once);
        parentBlockedByAdminLogServiceMock
            .Setup(x => x.SaveChangesLogAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(resultOfSavingToDb)
            .Verifiable(Times.Once);
        parentRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> f) => f.Invoke())
            .Verifiable(Times.Once);

        // Act
        var result = await parentService.BlockUnblockParent(parentBlockUnblockValid);

        // Assert
        Assert.AreEqual(expected.Value, result.Value);
    }

    [Test]
    public async Task BlockUnblockParent_WhenParentNotFoundInDb_ShouldReturnSuccess()
    {
        // Arrange
        var expected = Result<bool>.Success(true);

        BlockUnblockParentDto parentBlockUnblockValid = new()
        {
            ParentId = Guid.NewGuid(),
            IsBlocked = true,
            Reason = "Reason to block the parent",
        };
        Parent parent = null;

        parentRepositoryMock
            .Setup(x => x.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parent);

        // Act
        var result = await parentService.BlockUnblockParent(parentBlockUnblockValid);

        // Assert
        Assert.AreEqual(expected.Value, result.Value);
    }

    [Test]
    public async Task BlockUnblockParent_WhenParentAlreadyBlockedOrUnblocked_ShouldReturnSuccess()
    {
        // Arrange
        var expected = Result<bool>.Success(true);

        BlockUnblockParentDto parentBlockUnblockValid = new()
        {
            ParentId = Guid.NewGuid(),
            IsBlocked = true,
            Reason = "Reason to block the parent",
        };
        User parentUser = UserGenerator.Generate();
        parentUser.IsBlocked = true;
        Parent parent = new()
        {
            Id = parentBlockUnblockValid.ParentId,
            UserId = parentUser.Id,
            User = parentUser,
            IsDeleted = false,
        };

        parentRepositoryMock
            .Setup(x => x.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parent);

        // Act
        var result = await parentService.BlockUnblockParent(parentBlockUnblockValid);

        // Assert
        Assert.AreEqual(expected.Value, result.Value);
    }

    [Test]
    public void BlockUnblockParent_WhenBlockUnblockParentDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        BlockUnblockParentDto parentBlockUnblockValid = null;

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await parentService.BlockUnblockParent(parentBlockUnblockValid));
    }

    [Test]
    public void BlockUnblockParent_WhenSaveChangesAsyncIsFailed_ThrowInvalidOperationException()
    {
        // Arrange
        BlockUnblockParentDto parentBlockUnblockValid = new()
        {
            ParentId = Guid.NewGuid(),
            IsBlocked = true,
            Reason = "Reason to block the parent",
        };

        User parentUser = UserGenerator.Generate();
        parentUser.IsBlocked = false;
        Parent parent = new()
        {
            Id = parentBlockUnblockValid.ParentId,
            UserId = parentUser.Id,
            User = parentUser,
            IsDeleted = false,
        };
        var resultOfSavingToDb = 1;
        parentRepositoryMock
            .Setup(x => x.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parent)
            .Verifiable(Times.Once);
        parentRepositoryMock
            .Setup(x => x.SaveChangesAsync(true, default))
            .ThrowsAsync(new Exception())
            .Verifiable(Times.Once);
        parentBlockedByAdminLogServiceMock
            .Setup(x => x.SaveChangesLogAsync(
                parent.Id,
                currentUserServiceMock.Object.UserId,
                parentBlockUnblockValid.Reason,
                parentBlockUnblockValid.IsBlocked))
            .ReturnsAsync(resultOfSavingToDb)
            .Verifiable(Times.Never);
        parentRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> f) => f.Invoke())
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async ()
            => await parentService.BlockUnblockParent(parentBlockUnblockValid)
            .ConfigureAwait(false));
        Mock.VerifyAll();
    }

    [Test]
    public void BlockUnblockParent_WhenSaveChangesLogAsyncIsFailed_ThrowInvalidOperationException()
    {
        // Arrange
        BlockUnblockParentDto parentBlockUnblockValid = new()
        {
            ParentId = Guid.NewGuid(),
            IsBlocked = true,
            Reason = "Reason to block the parent",
        };

        User parentUser = UserGenerator.Generate();
        parentUser.IsBlocked = false;
        Parent parent = new()
        {
            Id = parentBlockUnblockValid.ParentId,
            UserId = parentUser.Id,
            User = parentUser,
            IsDeleted = false,
        };
        var resultOfSavingToDb = 1;
        parentRepositoryMock
            .Setup(x => x.GetByIdWithDetails(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parent)
            .Verifiable(Times.Once);
        parentRepositoryMock
            .Setup(x => x.SaveChangesAsync(true, default))
            .ReturnsAsync(resultOfSavingToDb)
            .Verifiable(Times.Once);
        parentBlockedByAdminLogServiceMock
            .Setup(x => x.SaveChangesLogAsync(
                parent.Id,
                currentUserServiceMock.Object.UserId,
                parentBlockUnblockValid.Reason,
                parentBlockUnblockValid.IsBlocked))
            .ThrowsAsync(new Exception())
            .Verifiable(Times.Once);
        parentRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> f) => f.Invoke())
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async ()
            => await parentService.BlockUnblockParent(parentBlockUnblockValid)
            .ConfigureAwait(false));
        Mock.VerifyAll();
    }
    #endregion

    #region Delete
    [Test]
    public void Delete_WhenIdIsNotValid_ThrowException()
    {
        // Arrange
        Guid parentId = Guid.NewGuid();
        currentUserServiceMock.Setup(x => x.UserHasRights(It.IsAny<ParentRights>())).Returns(() => Task.FromResult(true));
        parentRepositoryMock.Setup(r => r.GetById(parentId)).ReturnsAsync(null as Parent);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await parentService.Delete(parentId).ConfigureAwait(false));
    }

    [Test]
    public async Task Delete_WhenIdIsValid_ShouldCallDeleteMethods()
    {
        // Arrange
        Parent parent = ParentGenerator.Generate();
        var parentId = parent.Id;

        currentUserServiceMock.Setup(x => x.UserHasRights(It.IsAny<ParentRights>())).Returns(() => Task.FromResult(true));
        parentRepositoryMock.Setup(r => r.GetById(parentId)).ReturnsAsync(parent);
        parentRepositoryMock.Setup(r => r.RunInTransaction(It.IsAny<Func<Task>>()))
            .Returns((Func<Task> f) => f.Invoke());

        // Act
        await parentService.Delete(parentId).ConfigureAwait(false);

        // Assert
        Mock.VerifyAll();
    }
    #endregion

    #region GetByUserId
    [Test]
    public async Task GetByUserId_WhenUserHasRightsAndParentExists_ShouldReturnParentDtoResult()
    {
        // Arrange
        Parent parent = ParentGenerator.Generate();
        var userId = parent.UserId;
        var parents = new List<Parent>() { parent };

        currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<ParentRights>()))
            .Returns(() => Task.FromResult(true))
            .Verifiable(Times.Once);
        parentRepositoryMock
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(),
                "",
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parents.AsEnumerable())
            .Verifiable(Times.Once);

        // Act
        var result = await parentService.GetByUserId(userId).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ParentDTO>(result);
        Mock.VerifyAll();
    }
    #endregion

    #region GetPersonalInfoByUserId
    [Test]
    public async Task GetPersonalInfoByUserId_WhenUserExists_ShouldReturnShortUserDto()
    {
        // Arrange
        Parent parent = ParentGenerator.Generate();
        var userId = parent.UserId;
        var parents = new List<Parent>() { parent };

        parentRepositoryMock
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(),
                "",
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parents.AsEnumerable())
            .Verifiable(Times.Once);

        // Act
        var result = await parentService.GetPersonalInfoByUserId(userId).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ShortUserDto>(result);
        Mock.VerifyAll();
    }

    [TestCase(null)]
    [TestCase("")]
    public void GetPersonalInfoByUserId_WhenUserIdIsEmpty_ShouldThrowArgumentException(string userId)
    {
        // Arrange
        Parent parent = ParentGenerator.Generate();
        var parents = new List<Parent>() { parent };

        parentRepositoryMock
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(),
                "",
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parents.AsEnumerable())
            .Verifiable(Times.Never);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await parentService.GetPersonalInfoByUserId(userId).ConfigureAwait(false));
        Mock.VerifyAll();
    }
    #endregion

    #region Update
    [Test]
    public async Task Update_WhenUserHasRightsAndParentExists_ShouldReturnShortUserDto()
    {
        // Arrange
        var parent = ParentGenerator.Generate();
        parent.User = new();
        var userId = parent.UserId;
        var parents = new List<Parent>() { parent };
        var parentDto = new BaseUpdateUserDto
        {
            Id = userId,
            Email = "mail@gmail.com",
            PhoneNumber = "+380671234567"
        };

        parentRepositoryMock
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(),
                "",
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parents.AsEnumerable())
            .Verifiable(Times.Once);
        currentUserServiceMock
            .Setup(x => x.UserHasRights(It.IsAny<ParentRights>()))
            .Returns(() => Task.FromResult(true))
            .Verifiable(Times.Once);
        parentRepositoryMock
            .Setup(w => w.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<int>())
            .Verifiable(Times.Once);

        // Act
        var result = await parentService.Update(parentDto).ConfigureAwait(false);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOf<ShortUserDto>(result);
        Mock.VerifyAll();
    }

    [Test]
    public void Update_WhenParentNoExist_ShouldThrowArgumentException()
    {
        // Arrange
        var parents = new List<Parent>();
        var parentDto = new BaseUpdateUserDto
        {
            Id = "userId",
            Email = "mail@gmail.com",
            PhoneNumber = "+380671234567"
        };
        parentRepositoryMock
            .Setup(m => m.GetByFilter(
                It.IsAny<Expression<Func<Parent, bool>>>(),
                "",
                It.IsAny<Func<IQueryable<Parent>, IQueryable<Parent>>>()))
            .ReturnsAsync(parents.AsEnumerable())
            .Verifiable(Times.Once);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await parentService.Update(parentDto).ConfigureAwait(false));
        Mock.VerifyAll();
    }

    [Test]
    public void Update_WhenDtoIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var parentDto = (BaseUpdateUserDto)null;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await parentService.Update(parentDto).ConfigureAwait(false));
    }
    #endregion
}

//{
//    [TestFixture]
//    public class ParentServiceTests
//    {
//        private DbContextOptions<OutOfSchoolDbContext> options;
//        private OutOfSchoolDbContext context;
//        private IParentRepository repoParent;
//        private IEntityRepository<User> repoUser;
//        private IParentService service;
//        private Mock<IStringLocalizer<SharedResource>> localizer;
//        private Mock<ILogger<ParentService>> logger;

//        [SetUp]
//        public void SetUp()
//        {
//            var builder =
//                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
//                    databaseName: "OutOfSchoolTest")
//                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

//            options = builder.Options;
//            context = new OutOfSchoolDbContext(options);
//            localizer = new Mock<IStringLocalizer<SharedResource>>();
//            repoParent = new ParentRepository(context);
//            repoUser = new EntityRepository<User>(context);
//            logger = new Mock<ILogger<ParentService>>();
//            service = new ParentService(repoParent, repoUser, logger.Object, localizer.Object);

//            SeedDatabase();
//        }

//        [Test]
//        public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
//        {
//            // Arrange
//            var expected = new Parent() { UserId = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c" };

//            // Act
//            var result = await service.Create(expected.ToModel()).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected.UserId, result.UserId);
//        }

//        [Test]
//        public async Task GetAll_WhenCalled_ReturnsAllEntities()
//        {
//            // Arrange
//            var expected = await repoParent.GetAll();

//            // Act
//            var result = await service.GetAll().ConfigureAwait(false);

//            // Assert
//            Assert.That(expected.ToList().Count(), Is.EqualTo(result.Count()));
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task GetById_WhenIdIsValid_ReturnsEntity(long id)
//        {
//            // Arrange
//            var expected = await repoParent.GetById(id);

//            // Act
//            var result = await service.GetById(id).ConfigureAwait(false);

//            // Assert
//            Assert.AreEqual(expected.Id, result.Id);
//        }

//        [Test]
//        [TestCase(10)]
//        public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
//        {
//            // Act and Assert
//            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
//                async () => await service.GetById(id).ConfigureAwait(false));
//        }

//        [Test]
//        [TestCase("de909f35-5eb7-4b7a-bda8-40a5bfda96a6")]
//        public async Task GetByUserId_WhenIdIsValid_ReturnsEntities(string id)
//        {
//            // Arrange
//            var expected = await repoParent.GetByFilter(p => p.UserId == id);

//            // Act
//            var result = await service.GetByUserId(id).ConfigureAwait(false);

//            // Assert
//            result.Should().BeEquivalentTo(expected.FirstOrDefault().ToModel());
//        }

//        [Test]
//        [TestCase("fakeString")]
//        public void GetByUserId_WhenIdIsNotValid_TrowsArgumentException(string id)
//        {
//            // Act and Assert
//            service.Invoking(s => s.GetByUserId(id)).Should().ThrowAsync<ArgumentException>();
//        }

//        [Test]
//        public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
//        {
//            // Arrange
//            var changedEntity = new ShortUserDto()
//            {
//                Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
//                PhoneNumber = "1160327456",
//                LastName = "LastName",
//                MiddleName = "MiddleName",
//                FirstName = "FirstName",
//            };
//            Expression<Func<User, bool>> filter = p => p.Id == changedEntity.Id;

//            var users = repoUser.GetByFilterNoTracking(filter);

//            // Act
//            var result = await repoUser.Update(changedEntity.ToDomain(users.FirstOrDefault())).ConfigureAwait(false);

//            // Assert
//            Assert.That(changedEntity.FirstName, Is.EqualTo(result.FirstName));
//            Assert.That(changedEntity.LastName, Is.EqualTo(result.LastName));
//            Assert.That(changedEntity.MiddleName, Is.EqualTo(result.MiddleName));
//            Assert.That(changedEntity.PhoneNumber, Is.EqualTo(result.PhoneNumber));
//        }

//        [Test]
//        public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
//        {
//            // Arrange
//            var changedEntity = new ShortUserDto()
//            {
//               Id = "Invalid Id",
//               PhoneNumber = "1160327456",
//               LastName = "LastName",
//               MiddleName = "MiddleName",
//               FirstName = "FirstName",
//            };

//            // Act and Assert
//            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
//                async () => await service.Update(changedEntity).ConfigureAwait(false));
//        }

//        [Test]
//        [TestCase(1)]
//        public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
//        {
//            // Act
//            var countBeforeDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

//            context.Entry<Parent>(await repoParent.GetById(id).ConfigureAwait(false)).State = EntityState.Detached;

//            await service.Delete(id).ConfigureAwait(false);

//            var countAfterDeleting = (await service.GetAll().ConfigureAwait(false)).Count();

//            // Assert
//            Assert.That(countAfterDeleting, Is.Not.EqualTo(countBeforeDeleting));
//        }

//        [Test]
//        [TestCase(10)]
//        public void Delete_WhenIdIsInvalid_ThrowsDbUpdateConcurrencyException(long id)
//        {
//            // Act and Assert
//            Assert.ThrowsAsync<DbUpdateConcurrencyException>(
//                async () => await service.Delete(id).ConfigureAwait(false));
//        }

//        private void SeedDatabase()
//        {
//            using var context = new OutOfSchoolDbContext(options);
//            {
//                context.Database.EnsureDeleted();
//                context.Database.EnsureCreated();

//                var parents = new List<Parent>()
//                {
//                    new Parent() { Id = 1,  UserId = "de909f35-5eb7-4b7a-bda8-40a5bfda96a6" },
//                    new Parent() { Id = 2,  UserId = "de804f35-5eb7-4b8n-bda8-70a5tyfg96a6" },
//                    new Parent() { Id = 3,  UserId = "de804f35-bda8-4b8n-5eb7-70a5tyfg90a6" },
//                };

//                var user = new User()
//                {
//                    Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
//                    CreatingTime = default,
//                    LastLogin = default,
//                    MiddleName = "MiddleName",
//                    FirstName = "FirstName",
//                    LastName = "LastName",
//                    UserName = "user@gmail.com",
//                    NormalizedUserName = "USER@GMAIL.COM",
//                    Email = "user@gmail.com",
//                    NormalizedEmail = "USER@GMAIL.COM",
//                    EmailConfirmed = false,
//                    PasswordHash = "AQAAAAECcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
//                    SecurityStamp = "   CCCJIYDFRG236HXFKGYS7H6QT2DE2LFF",
//                    ConcurrencyStamp = "cb54f60f-6282-4416-874c-d1edce844d07",
//                    PhoneNumber = "0965679725",
//                    Role = "provider",
//                    PhoneNumberConfirmed = false,
//                    TwoFactorEnabled = false,
//                    LockoutEnabled = true,
//                    AccessFailedCount = 0,
//                    IsRegistered = false,
//                };

//                context.Parents.AddRangeAsync(parents);
//                context.Users.AddAsync(user);
//                context.SaveChangesAsync();
//            }
//        }
//    }
//}

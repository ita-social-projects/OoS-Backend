using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class UserServiceTest
{
    private DbContextOptions<OutOfSchoolDbContext> options;
    private TestOutOfSchoolDbContext context;
    private IEntityRepositorySoftDeleted<string, User> repo;
    private IUserService service;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<ILogger<UserService>> logger;
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new TestOutOfSchoolDbContext(options);
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        repo = new EntityRepositorySoftDeleted<string, User>(context);
        logger = new Mock<ILogger<UserService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileTypes<CommonProfile, MappingProfile>();
        service = new UserService(repo, logger.Object, localizer.Object, mapper);

        SeedDatabase();
    }

    [Test]
    [Order(1)]
    public async Task GetAll_WhenCalled_ReturnsAllEntities()
    {
        // Arrange
        var expected = await repo.GetAll();

        // Act
        var result = await service.GetAll().ConfigureAwait(false);

        // Assert
        Assert.That(expected.ToList().Count(), Is.EqualTo(result.Count()));
    }

    [Test]
    [Order(2)]
    [TestCase("cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c")]
    public async Task GetById_WhenIdIsValid_ReturnsEntity(string id)
    {
        // Arrange
        Expression<Func<User, bool>> filter = p => p.Id == id;
        var expected = await repo.GetByFilter(filter).ConfigureAwait(false);

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.FirstOrDefault().Id, result.Id);
    }

    [Test]
    [Order(3)]
    [TestCase("Invalid Id")]
    public void GetById_WhenIdIsInvalid_ThrowsArgumentException(string id)
    {
        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetById(id).ConfigureAwait(false));
    }

    [Test]
    public async Task Update_WhenEntityIsValid_UpdatesExistedEntity()
    {
        // Arrange
        var changedEntity = new ShortUserDto()
        {
            Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
            PhoneNumber = "1160327456",
            LastName = "LastName",
            MiddleName = "MiddleName",
            FirstName = "FirstName",
        };
        Expression<Func<User, bool>> filter = p => p.Id == changedEntity.Id;

        var users = repo.GetByFilterNoTracking(filter);

        // Act
        var result = await repo.Update(mapper.Map(changedEntity, users.FirstOrDefault())).ConfigureAwait(false);

        // Assert
        Assert.That(changedEntity.FirstName, Is.EqualTo(result.FirstName));
        Assert.That(changedEntity.LastName, Is.EqualTo(result.LastName));
        Assert.That(changedEntity.MiddleName, Is.EqualTo(result.MiddleName));
        Assert.That(changedEntity.PhoneNumber, Is.EqualTo(result.PhoneNumber));
    }

    [Test]
    public void Update_WhenEntityIsInvalid_ThrowsDbUpdateConcurrencyException()
    {
        // Arrange
        var changedEntity = new ShortUserDto()
        {
            Id = "Invalid Id",
            PhoneNumber = "1160327456",
            LastName = "LastName",
            MiddleName = "MiddleName",
            FirstName = "FirstName",
        };

        // Act and Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () => await service.Update(changedEntity).ConfigureAwait(false));
    }

    [Test]
    [TestCase("CVc4a6876a-77fb-4ecnne-9c78-a0880286ae3c")]
    public async Task Delete_WhenIdIsValid_CalledDeleteMethod(string id)
    {
        // Arrange
        var users = await service.GetAll().ConfigureAwait(false);

        // Act
        await service.Delete(id);

        // Assert
        Assert.AreEqual(users.Count() - 1, (await service.GetAll().ConfigureAwait(false)).Count());
    }

    [Test]
    public void Delete_WhenIdIsInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidId = "invalidId";

        // Act and Assert
        Assert.ThrowsAsync<ArgumentException>(() => service.Delete(invalidId));
    }

    private void SeedDatabase()
    {
        using var context = new TestOutOfSchoolDbContext(options);
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var users = new List<User>()
        {
            new User()
            {
                Id = "cqQQ876a-BBfb-4e9e-9c78-a0880286ae3c",
                CreatingTime = default,
                LastLogin = default,
                MiddleName = "MiddleName1",
                FirstName = "FirstName1",
                LastName = "LastName1",
                UserName = "user1@gmail.com",
                NormalizedUserName = "USER1@GMAIL.COM",
                Email = "user1@gmail.com",
                NormalizedEmail = "USER1@GMAIL.COM",
                EmailConfirmed = false,
                PasswordHash = "AQAAAAECcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                SecurityStamp = "   CCCJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                ConcurrencyStamp = "cb54f60f-6282-4416-874c-d1edce844d07",
                PhoneNumber = "0965679725",
                Role = "parent",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
            },
            new User()
            {
                Id = "MM4a6876a-77fb-4bvse-9c78-a0880286ae3c",
                CreatingTime = default,
                LastLogin = default,
                MiddleName = "MiddleName2",
                FirstName = "FirstName2",
                LastName = "LastName2",
                UserName = "user2@gmail.com",
                NormalizedUserName = "USER2@GMAIL.COM",
                Email = "user2@gmail.com",
                NormalizedEmail = "USER2@GMAIL.COM",
                EmailConfirmed = false,
                PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce822d07",
                PhoneNumber = "0965679000",
                Role = "parent",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
            },
            new User()
            {
                Id = "c4a6876a-77fb-4e9e-9c78-a0880286aeV0",
                CreatingTime = default,
                LastLogin = default,
                MiddleName = "MiddleName3",
                FirstName = "FirstName3",
                LastName = "LastName3",
                UserName = "user3@gmail.com",
                NormalizedUserName = "USER3@GMAIL.COM",
                Email = "user3@gmail.com",
                NormalizedEmail = "USER3@GMAIL.COM",
                EmailConfirmed = false,
                PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pMWRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                SecurityStamp = "WGWJIYCCRG236HXFKGYS7H6QT2DE2LFF",
                ConcurrencyStamp = "cb54f60f-70982-4416-926c-d1edce844d07",
                PhoneNumber = "0675679312",
                Role = "provider",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
            },
            new User()
            {
                Id = "CEc4a6876a-77fb-4e9e-9c78-a0880286ae3c",
                CreatingTime = default,
                LastLogin = default,
                MiddleName = "MiddleName4",
                FirstName = "FirstName4",
                LastName = "LastName4",
                UserName = "user4@gmail.com",
                NormalizedUserName = "USER4@GMAIL.COM",
                Email = "user4@gmail.com",
                NormalizedEmail = "USER4@GMAIL.COM",
                EmailConfirmed = false,
                PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce844d07",
                PhoneNumber = "0965679312",
                Role = "provider",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
            },
            new User()
            {
                Id = "CVc4a6876a-77fb-4ecnne-9c78-a0880286ae3c",
                CreatingTime = default,
                LastLogin = default,
                MiddleName = "MiddleName5",
                FirstName = "FirstName5",
                LastName = "LastName5",
                UserName = "user5@gmail.com",
                NormalizedUserName = "USER5@GMAIL.COM",
                Email = "user5@gmail.com",
                NormalizedEmail = "USER5@GMAIL.COM",
                EmailConfirmed = false,
                PasswordHash = "AQAAAAEAACcQAAAAEPXMPMbzuDZIKJUN4pBhRWMtf35Q3RN4QOll7UfnTdmfXHEcgswabznBezJmeTMvEw==",
                SecurityStamp = "WGWJIYDFRG236HXFKGYS7H6QT2DE2LFF",
                ConcurrencyStamp = "cb54f60f-6282-4416-926c-d1edce844d07",
                PhoneNumber = "0965889312",
                Role = "provider",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
            },
        };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
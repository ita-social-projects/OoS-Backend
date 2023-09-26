﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models.SocialGroup;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class SocialGroupServiceTests
{
    private ISocialGroupService service;
    private OutOfSchoolDbContext context;
    private IEntityRepositorySoftDeleted<long, SocialGroup> repository;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<ILogger<SocialGroupService>> logger;
    private DbContextOptions<OutOfSchoolDbContext> options;
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var builder =
            new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                databaseName: "OutOfSchoolTestDB");

        options = builder.Options;
        context = new OutOfSchoolDbContext(options);
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        repository = new EntityRepositorySoftDeleted<long, SocialGroup>(context);
        logger = new Mock<ILogger<SocialGroupService>>();
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        service = new SocialGroupService(repository, logger.Object, localizer.Object, mapper);

        SeedDatabase();
    }

    [Test]
    public async Task GetAll_WhenCalled_ReturnsAllSocialGroups()
    {
        // Arrange
        var expected = await repository.GetAll();

        // Act
        var result = await service.GetAll().ConfigureAwait(false);

        // Assert
        Assert.AreEqual(result.ToList().Count(), expected.Count());
    }

    [Test]
    [TestCase(1)]
    public async Task GetById_WhenIdIsValid_ReturnsSocialGroup(long id)
    {
        // Arrange
        var expected = await repository.GetById(id);

        // Act
        var result = await service.GetById(id).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Id, result.Id);
    }

    [Test]
    [TestCase(10)]
    public void GetById_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
    {
        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.GetById(id).ConfigureAwait(false));
    }

    [Test]
    public async Task Create_WhenEntityIsValid_ReturnsCreatedEntity()
    {
        // Arrange
        var expected = new SocialGroupCreate() { Name = "ТестІмя", NameEn = "TestName" };

        // Act
        var result = await service.Create(expected).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(expected.Name, result.Name);
        Assert.AreEqual(expected.NameEn, result.NameEn);
    }

    [Test]
    [TestCase(LocalizationType.Ua)]
    public async Task Update_WhenEntityIsValidUa_UpdatesExistedEntity(LocalizationType localization)
    {
        // Arrange
        var changedEntity = new SocialGroupDto()
        {
            Id = 1,
            Name = "ТестІмя",
        };

        // Act
        var result = await service.Update(changedEntity, localization).ConfigureAwait(false);

        // Assert
        Assert.That(changedEntity.Name, Is.EqualTo(result.Name));
    }

    [Test]
    [TestCase(LocalizationType.En)]
    public async Task Update_WhenEntityIsValidEn_UpdatesExistedEntity(LocalizationType localization)
    {
        // Arrange
        var changedEntity = new SocialGroupDto()
        {
            Id = 1,
            Name = "TestName",
        };

        // Act
        var result = await service.Update(changedEntity, localization).ConfigureAwait(false);

        // Assert
        Assert.That(changedEntity.Name, Is.EqualTo(result.Name));
    }

    [Test]
    [TestCase(1)]
    public async Task Delete_WhenIdIsValid_DeletesEntity(long id)
    {
        // Arrange
        var expected = await context.SocialGroups.CountAsync();

        // Act
        await service.Delete(id).ConfigureAwait(false);

        var result = (await service.GetAll().ConfigureAwait(false)).Count();

        // Assert
        Assert.AreEqual(expected - 1, result);
    }

    [Test]
    [TestCase(10)]
    public void Delete_WhenIdIsInvalid_ThrowsArgumentOutOfRangeException(long id)
    {
        // Act and Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await service.Delete(id).ConfigureAwait(false));
    }

    private void SeedDatabase()
    {
        using var context = new OutOfSchoolDbContext(options);
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var socialGroups = new List<SocialGroup>()
        {
            new SocialGroup { Name = "NoName", },
            new SocialGroup { Name = "HaveName", },
            new SocialGroup { Name = "MissName", },
        };

            context.SocialGroups.AddRange(socialGroups);
            context.SaveChanges();
        }
    }
}
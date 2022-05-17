﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services
{
    [TestFixture]
    public class CompanyInformationServiceTests
    {
        private DbContextOptions<OutOfSchoolDbContext> options;
        private OutOfSchoolDbContext context;
        private IEntityRepository<CompanyInformation> repository;
        private ICompanyInformationService service;
        private Mock<ILogger<CompanyInformationService>> logger;
        private Mock<IMapper> mapper;

        [SetUp]
        public void SetUp()
        {
            var builder =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(
                    databaseName: "OutOfSchoolTestDB");

            options = builder.Options;
            context = new OutOfSchoolDbContext(options);

            logger = new Mock<ILogger<CompanyInformationService>>();
            mapper = new Mock<IMapper>();
            repository = new EntityRepository<CompanyInformation>(context);
            service = new CompanyInformationService(repository, logger.Object, mapper.Object);

            SeedDatabase();
        }

        [Test]
        [TestCase(CompanyInformationType.AboutPortal)]
        [TestCase(CompanyInformationType.SupportInformation)]
        public async Task GetByType_WhenTypeIsValid_ReturnsEntity(CompanyInformationType type)
        {
            // Arrange
            Expression<Func<CompanyInformation, bool>> filter = p => p.Type == type;
            var expected = await repository.GetByFilter(filter, "CompanyInformationItems").ConfigureAwait(false);

            mapper.Setup(m => m.Map<CompanyInformationDto>(It.IsAny<CompanyInformation>())).Returns(expected.FirstOrDefault().ToModel());

            // Act
            var result = await service.GetByType(type).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(expected.FirstOrDefault().Type, result.Type);
        }

        [Test]
        [TestCase(CompanyInformationType.LawsAndRegulations)]
        public async Task GetByType_WhenTypeIsValid_EntityNotExists(CompanyInformationType type)
        {
            // Arrange
            Expression<Func<CompanyInformation, bool>> filter = p => p.Type == type;
            var expected = await repository.GetByFilter(filter, "CompanyInformationItems").ConfigureAwait(false);

            mapper.Setup(m => m.Map<CompanyInformationDto>(It.IsAny<CompanyInformation>())).Returns(expected.FirstOrDefault().ToModel());

            // Act
            var result = await service.GetByType(type).ConfigureAwait(false);

            // Assert
            Assert.IsFalse(expected.Any());
            Assert.IsNull(result);
        }

        [Test]
        [TestCase(CompanyInformationType.AboutPortal)]
        public async Task Update_UpdatesExistedEntity(CompanyInformationType type)
        {
            // Arrange
            var changedEntity = GetEntityDto(type);

            // Act
            mapper.Setup(m => m.Map<IEnumerable<CompanyInformationItem>>(It.IsAny<IEnumerable<CompanyInformationItemDto>>())).Returns(changedEntity.ToDomain().CompanyInformationItems);
            mapper.Setup(m => m.Map<CompanyInformationDto>(It.IsAny<CompanyInformation>())).Returns(changedEntity);

            var result = await service.Update(changedEntity, type);

            // Assert
            Assert.That(changedEntity.Title, Is.EqualTo(result.Title));
            Assert.That(changedEntity.Type, Is.EqualTo(result.Type));
            Assert.That(changedEntity.CompanyInformationItems.Count(), Is.EqualTo(result.CompanyInformationItems.Count()));
            Assert.That(changedEntity.CompanyInformationItems.First().SectionName, Is.EqualTo(result.CompanyInformationItems.First().SectionName));
            Assert.That(changedEntity.CompanyInformationItems.Last().Description, Is.EqualTo(result.CompanyInformationItems.Last().Description));
        }

        [Test]
        [TestCase(CompanyInformationType.LawsAndRegulations)]
        public async Task Update_UpdatesNotExistedEntity(CompanyInformationType type)
        {
            // Arrange
            var changedEntity = GetEntityDto(type);

            // Act
            mapper.Setup(m => m.Map<IEnumerable<CompanyInformationItem>>(It.IsAny<IEnumerable<CompanyInformationItemDto>>())).Returns(changedEntity.ToDomain().CompanyInformationItems);
            mapper.Setup(m => m.Map<CompanyInformation>(It.IsAny<CompanyInformationDto>())).Returns(changedEntity.ToDomain());
            mapper.Setup(m => m.Map<CompanyInformationDto>(It.IsAny<CompanyInformation>())).Returns(changedEntity);

            var result = await service.Update(changedEntity, type);

            // Assert
            Assert.That(changedEntity.Title, Is.EqualTo(result.Title));
            Assert.That(changedEntity.Type, Is.EqualTo(result.Type));
            Assert.That(changedEntity.CompanyInformationItems.Count(), Is.EqualTo(result.CompanyInformationItems.Count()));
            Assert.That(changedEntity.CompanyInformationItems.First().SectionName, Is.EqualTo(result.CompanyInformationItems.First().SectionName));
            Assert.That(changedEntity.CompanyInformationItems.Last().Description, Is.EqualTo(result.CompanyInformationItems.Last().Description));
        }

        private CompanyInformationDto GetEntityDto(CompanyInformationType type)
        {
            return new CompanyInformationDto()
            {
                Title = "Title",
                Type = type,
                CompanyInformationItems = new List<CompanyInformationItemDto>
                        {
                            new CompanyInformationItemDto()
                            {
                                SectionName = "Section 11",
                                Description = "Description 11",
                            },
                            new CompanyInformationItemDto()
                            {
                                SectionName = "Section 21",
                                Description = "Description 21",
                            },
                        },
            };
        }

        private void SeedDatabase()
        {
            using var dbContext = new OutOfSchoolDbContext(options);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var companyInformations = new List<CompanyInformation>()
                {
                   new CompanyInformation()
                   {
                        Id = Guid.NewGuid(),
                        Title = "About Portal",
                        Type = OutOfSchool.Services.Enums.CompanyInformationType.AboutPortal,
                        CompanyInformationItems = new List<CompanyInformationItem>
                        {
                            new CompanyInformationItem()
                            {
                                SectionName = "AP: Section 1",
                                Description = "AP: Description 1",
                            },
                        },
                   },
                   new CompanyInformation()
                   {
                        Id = Guid.NewGuid(),
                        Title = "Support Information",
                        Type = OutOfSchool.Services.Enums.CompanyInformationType.SupportInformation,
                        CompanyInformationItems = new List<CompanyInformationItem>
                        {
                            new CompanyInformationItem()
                            {
                                SectionName = "SI: Section 1",
                                Description = "SI: Description 1",
                            },
                        },
                   },
                };

            dbContext.CompanyInformation.AddRangeAsync(companyInformations);
            dbContext.SaveChangesAsync();
        }
    }
}

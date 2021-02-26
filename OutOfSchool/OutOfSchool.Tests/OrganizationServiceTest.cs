using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Implementation;
using OutOfSchool.WebApi.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Tests
{
    [TestFixture]
    public class OrganizationServiceTest
    {
        private readonly OutOfSchoolDbContext _context;
        private readonly OrganizationRepository organizationRepository;
        private readonly  OrganizationService _organizationService;
        private readonly IMapper mapper;

        public OrganizationServiceTest()
        {
            mapper = new MapperConfiguration(x => x.AddProfile(new OrganizationMapperProfile()))
               .CreateMapper();

            var dbContextOptions =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

            _context = new OutOfSchoolDbContext(dbContextOptions);

            organizationRepository = new OrganizationRepository(_context);
            _organizationService = new OrganizationService(organizationRepository, mapper);
        }
       

        [OneTimeTearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        [Test,Order(1)]
        public async Task Create_Organization_ReturnsCreatedOrganization()
        {
            // Arrange
            var newOrganization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Facebook",
                Instagram = "Instagram",
                Description = "Description",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",              
                Type = OrganizationType.FOP,
                UserId = "123"
            };

            //Act
            var organization = await _organizationService.Create(newOrganization).ConfigureAwait(false);
           
            //Assert
            Assert.That(newOrganization.Id, Is.EqualTo(organization.Id), "Id's are equal");
            Assert.That(newOrganization.Title, Is.EqualTo(organization.Title), "Titles are equal");                   
        }

        [Test, Order(3)]
        public void Create_Organization_ReturnsArgumentNullException()
        {
            // Arrange
            OrganizationDTO organization = null;

            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _organizationService.Create(organization).ConfigureAwait(false));
        }

        [Test, Order(4)]
        public void Create_NotUniqueOrganization_ReturnsArgumentException()
        {
            // Arrange
            var newOrganization = new OrganizationDTO
            {
                Id = 10,
                Title = "bla",
                Website = "Website",
                Facebook = "Facebook",
                Instagram = "Instagram",
                Description = "Description",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = OrganizationType.TOV,
                UserId = "123"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _organizationService.Create(newOrganization).ConfigureAwait(false));           
        }

        [Test, Order(5)]
        public async Task GetAll_Organizations_ReturnsSameAmountOrganizations()
        {
            // Arrange
            var allOrganizations = await _organizationService.GetAll();

            // Act
            var amountOrganizations = allOrganizations.ToList().Count;

            // Assert
            Assert.AreEqual(_context.Organizations.ToList().Count, amountOrganizations);
        }

        [Test, Order(2)]
        public async Task Get_OrganizationById_ReturnsOrganizationWithSameId()
        {
            // Arrange
            long id = 0b101;

            // Act
            var organization = await _organizationService.GetById(id).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(id, organization.Id);
        }

        [Test, Order(6)]
        public async Task Get_OrganizationById_ReturnsArgumentException()
        {
            // Arrange
            long nonexistentId = 1234;

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _organizationService.GetById(nonexistentId).ConfigureAwait(false));
        }


        [Test,Order(7)]
        public async Task Update_Organization_ReturnsUpdatedOrganization()
        {
            // Arrange
            var oldOrganization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "Description",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = OrganizationType.FOP,
                UserId = "123"
            };

            _context.Entry<Organization>(await organizationRepository.GetById(oldOrganization.Id)).State = EntityState.Detached;

            // Act
            var updatedOrganization = await _organizationService.Update(oldOrganization).ConfigureAwait(false);

            // Assert
            Assert.That(oldOrganization.Facebook, Is.EqualTo(updatedOrganization.Facebook));
            Assert.That(oldOrganization.Instagram, Is.EqualTo(updatedOrganization.Instagram));
        }

        [Test, Order(8)]
        public async Task Update_Organization_ReturnsArgumentNullException()
        {
            // Arrange
            OrganizationDTO oldOrganization = null;

            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                  async () => await _organizationService.Update(oldOrganization).ConfigureAwait(false));     
        }

        [Test, Order(9)]
        public async Task Update_OrganizationWithEmptyEDRPOU_ReturnsArgumentException()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "Description",
                MFO = "123456",
                EDRPOU = string.Empty,
                INPP = "1234567891",
                Type = OrganizationType.FOP,
                UserId = "123"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(organization).ConfigureAwait(false));
        }

        [Test, Order(10)]
        public async Task Update_OrganizationWithEmptyINPP_ReturnsArgumentException()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "Description",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = string.Empty,
                Type = OrganizationType.FOP,
                UserId = "123"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(organization).ConfigureAwait(false));
        }

        [Test, Order(11)]
        public async Task Update_OrganizationWithEmptyMFO_ReturnsArgumentException()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "Description",
                MFO = string.Empty,
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = OrganizationType.FOP,
                UserId = "123"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(organization).ConfigureAwait(false));
        }

        [Test, Order(12)]
        public async Task Update_OrganizationWithEmptyTitle_ReturnsArgumentException()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = string.Empty,
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "Description",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = OrganizationType.FOP,
                UserId = "123"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(organization).ConfigureAwait(false));
        }

        [Test, Order(13)]
        public async Task Update_OrganizationWithEmptyDescription_ReturnsArgumentException()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = string.Empty,
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = OrganizationType.FOP,
                UserId = "123"
            };

            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(organization).ConfigureAwait(false));
        }

        [Test, Order(14)]
        public async Task Delete_Organization_DeletedFromDatabase()
        {
            // Arrange
            long organizationId = 0b101;

            _context.Entry<Organization>(await organizationRepository.GetById(organizationId)).State = EntityState.Detached;

            // Act
            await _organizationService.Delete(organizationId);
            var organizations = await _organizationService.GetAll();

            // Assert
            Assert.AreEqual(_context.Organizations.ToList().Count, organizations.ToList().Count);
        }



    }

}

   




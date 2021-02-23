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
        private OutOfSchoolDbContext _context;
        private OrganizationRepository organizationRepository;
        private readonly IMapper mapper = new MapperConfiguration(x => x.AddProfile(new OrganizationMapperProfile())).CreateMapper();
        private OrganizationService _organizationService;


        [OneTimeSetUp]
        public void Setup()
        {
            var dbContextOptions =
                new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase("TestDB").UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            _context = new OutOfSchoolDbContext(dbContextOptions.Options);
           
            organizationRepository = new OrganizationRepository(_context);
            _organizationService = new OrganizationService(organizationRepository, mapper);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        [Test,Order(1)]
        public async Task CreateOrganization_Succsess()
        {
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
                UserId = 0xFF
            };

            var organization = await _organizationService.Create(newOrganization).ConfigureAwait(false);
           
            Assert.That(newOrganization.Id, Is.EqualTo(organization.Id), "Id`s are equal");
            Assert.That(newOrganization.Title, Is.EqualTo(organization.Title), "Titles are equal");                   
        }

        [Test, Order(3)]
        public void CreateOrganization_ArgumentNullException()
        {

            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _organizationService.Create(null).ConfigureAwait(false));
        }

        [Test, Order(4)]
        public void CreateOrganization_CheckForUniqueness()
        {
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
                UserId = 245
            };

            Assert.ThrowsAsync<ArgumentException>(
                async () => await _organizationService.Create(newOrganization).ConfigureAwait(false));
            
        }

        [Test, Order(5)]
        public async Task GetAllOrganizations_Succsess()
        {          

            var organizations = await _organizationService.GetAll();

            Assert.AreEqual(_context.Organizations.ToList().Count, organizations.ToList().Count);
        }

        [Test, Order(2)]
        public async Task GetOrganizationById_Succsess()
        {          
            var organization = await _organizationService.GetById(0b101).ConfigureAwait(false);

            Assert.AreEqual(0b101, organization.Id);
        }

        [Test, Order(6)]
        public async Task GetOrganizationById_ArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(
                async () => await _organizationService.GetById(1111).ConfigureAwait(false));
        }


        [Test,Order(7)]
        public async Task UpdateOrganization_Sucsess()
        {
            var newOrganization = new OrganizationDTO
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
                UserId = 0xFF
            };

            _context.Entry<Organization>(await organizationRepository.GetById(newOrganization.Id)).State = EntityState.Detached;

            var organization = await _organizationService.Update(newOrganization).ConfigureAwait(false);

            Assert.That(newOrganization.Facebook, Is.EqualTo(organization.Facebook));
            Assert.That(newOrganization.Instagram, Is.EqualTo(organization.Instagram));
        }

        [Test, Order(8)]
        public async Task UpdateOrganization_ArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                  async () => await _organizationService.Update(null).ConfigureAwait(false));     
        }

        [Test, Order(9)]
        public async Task UpdateOrganization_EmptyEDRPOU()
        {
            var newOrganization = new OrganizationDTO
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
                UserId = 0xFF
            };         

            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(newOrganization).ConfigureAwait(false));
        }

        [Test, Order(10)]
        public async Task UpdateOrganization_EmptyINPP()
        {
            var newOrganization = new OrganizationDTO
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
                UserId = 0xFF
            };

            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(newOrganization).ConfigureAwait(false));
        }

        [Test, Order(11)]
        public async Task UpdateOrganization_EmptyMFO()
        {
            var newOrganization = new OrganizationDTO
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
                UserId = 0xFF
            };

            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(newOrganization).ConfigureAwait(false));
        }

        [Test, Order(12)]
        public async Task UpdateOrganization_EmptyTitle()
        {
            var newOrganization = new OrganizationDTO
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
                UserId = 0xFF
            };

            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(newOrganization).ConfigureAwait(false));
        }

        [Test, Order(13)]
        public async Task UpdateOrganization_EmptyDescription()
        {
            var newOrganization = new OrganizationDTO
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
                UserId = 0xFF
            };

            Assert.ThrowsAsync<ArgumentException>(
                   async () => await _organizationService.Update(newOrganization).ConfigureAwait(false));
        }

        [Test, Order(14)]
        public async Task DeleteOrganization_Sucsess()
        {          
            long id = 0b101;

            _context.Entry<Organization>(await organizationRepository.GetById(id)).State = EntityState.Detached;

            await _organizationService.Delete(id);
            var organizations = await _organizationService.GetAll();

            Assert.AreEqual(_context.Organizations.ToList().Count, organizations.ToList().Count);
        }



    }

}

   




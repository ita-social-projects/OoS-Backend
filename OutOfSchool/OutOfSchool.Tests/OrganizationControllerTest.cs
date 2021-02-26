
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace OutOfSchool.Tests
{
    [TestFixture]
    public class OrganizationControllerTest
    {
        private readonly ILogger<OrganizationController> _logger;
        private readonly Mock<IOrganizationService> organizationService;
        private readonly OrganizationController organizationController;
        private readonly ClaimsPrincipal user;
        public OrganizationControllerTest()
        {
            _logger = new Mock<ILogger<OrganizationController>>().Object;
            organizationService = new Mock<IOrganizationService>();
            organizationController = new OrganizationController(_logger, organizationService.Object);
            user = new ClaimsPrincipal(new ClaimsIdentity());
            organizationController.ControllerContext.HttpContext = new DefaultHttpContext { User = user };
        }
     
        [Test]
        public async Task Get_Organizations_ReturnsOkObjectResult()
        {
            // Arrange
            IEnumerable<OrganizationDTO> organizations = new List<OrganizationDTO>
            {
                new OrganizationDTO
                {
                      Id = 0b101,
                      Title = "Title",
                      Website = "Website",
                      Facebook = "Фейсбук",
                      Instagram = "Инстаграм",
                      Description = "ererer",
                      MFO = "123456",
                      EDRPOU = "12345678",
                      INPP = "1234567891",
                      Type = 0,
                      UserId = "123",
                },
            }.ToList();

            organizationService.Setup( it => it.GetAll()).Returns(Task.FromResult(organizations));

            // Act
            var response = await organizationController.GetOrganizations();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(response);
        }

        [Test]
        public async Task Get_Organizations_ReturnsBadRequestObjectResult()
        {
            // Arrange
            organizationService.Setup(it => it.GetAll()).Throws(new Exception());

            // Act
            var response = await organizationController.GetOrganizations();

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }

        [Test]
        public async Task Get_OrganizationById_ReturnsOkObjectResult()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "ererer",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = 0,
                UserId = "123"
            };       
          
            organizationService.Setup(it => it.GetById(organization.Id)).Returns(Task.FromResult(organization));

            // Act
            var response = await organizationController.GetOrganizationById(organization.Id);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(response);
        }

        [Test]
        public async Task Get_OrganizationById_ReturnsBadRequestObjectResult()
        {
            // Arrange
            long organizationId = 123;

            organizationService.Setup(it => it.GetById(organizationId)).Throws(new ArgumentException());

            // Act
            var response = await organizationController.GetOrganizationById(organizationId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }

        [Test]
        public async Task Update_Organization_ReturnOkObjectResult()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "ererer",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = 0,
                UserId = "123"
            };
            
            organizationService.Setup(it => it.Update(organization)).Returns(Task.FromResult(organization));

            // Act
            var result = await organizationController.Update(organization);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Update_OrganizationNotValidModel_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "ererer",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = 0,
                UserId = "123"
            };

            organizationController.ModelState.AddModelError("fakeError", "fakeError");

            organizationService.Setup(it => it.Update(organization)).Returns(Task.FromResult(organization));

            // Act
            var response = await organizationController.Update(organization);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(response);
        }
        [Test]
        public async Task Update_OrganizationThatNull_ReturnsBadRequestObjectResult()
        {
            // Arrange
            OrganizationDTO organization = null;
            
            organizationService.Setup(it => it.Update(organization)).Returns(Task.FromResult(organization));

            // Act
            var responce = await organizationController.Update(organization);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(responce);
        }

        [Test]
        public async Task Update_OrganizationExceptionOccurred_ReturnsBadRequestObjectResult()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "ererer",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = 0,
                UserId = "123"
            };
        
            organizationService.Setup(it => it.Update(organization)).Throws(new Exception());

            // Act
            var responce = await organizationController.Update(organization);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(responce);
        }

        [Test]
        public async Task Delete_Organization_ReturnsOkResult()
        {
            // Arrange
            long organizationId = 123;

            organizationService.Setup(it => it.Delete(organizationId));

            // Act
            var responce = await organizationController.Delete(organizationId);

            // Assert
            Assert.IsInstanceOf<OkResult>(responce);
        }

        [Test]
        public async Task Delete_OrganizationExceptionOccurred_ReturnsBadRequestObjectResult()
        {
            // Arrange
            long organizationId = 123;

            organizationService.Setup(it => it.Delete(organizationId)).Throws(new Exception());

            // Act
            var responce = await organizationController.Delete(organizationId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(responce);
        }


        [Test]
        public async Task Create_Organization_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var organization = new OrganizationDTO
            {
                Id = 0b101,
                Title = "Title",
                Website = "Website",
                Facebook = "Фейсбук",
                Instagram = "Инстаграм",
                Description = "ererer",
                MFO = "123456",
                EDRPOU = "12345678",
                INPP = "1234567891",
                Type = 0,           
            };
            
            organizationService.Setup(it => it.Create(organization)).Returns(Task.FromResult(organization));

            // Act
            var response = await organizationController.CreateOrganization(organization);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(response);
        }     
    }
}
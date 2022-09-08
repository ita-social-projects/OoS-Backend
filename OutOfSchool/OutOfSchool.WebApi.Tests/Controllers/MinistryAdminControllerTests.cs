using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class MinistryAdminControllerTests
{
    private MinistryAdminController ministryAdminController;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private IMapper mapper;
    private MinistryAdminDto ministryAdminDto;
    private List<MinistryAdminDto> ministryAdminDtos;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileType<Util.MappingProfile>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        ministryAdminController =
            new MinistryAdminController(ministryAdminServiceMock.Object, new Mock<ILogger<MinistryAdminController>>().Object);
        ministryAdminDto = AdminGenerator.GenerateMinistryAdminDto();
        ministryAdminDtos = AdminGenerator.GenerateMinistryAdminsDtos(10);
    }

    [Test]
    public async Task GetById_WhenIdIsValid_ShouldReturnOkResultObject()
    {
        // Arrange
        ministryAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(ministryAdminDto);

        // Act
        var result = await ministryAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        ministryAdminServiceMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreSame(ministryAdminDto, result.Value);
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetById_WhenNoMinistryAdminWithSuchId_ReturnsNotFoundResult()
    {
        // Arrange
        ministryAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null as MinistryAdminDto);

        // Act
        var result = await ministryAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        ministryAdminServiceMock.VerifyAll();
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    [Test]
    public async Task GetMinistryAdmins_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        var expected = new SearchResult<MinistryAdminDto>
        {
            TotalAmount = 10,
            Entities = ministryAdminDtos.Select(x => mapper.Map<MinistryAdminDto>(x)).ToList(),
        };

        ministryAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await ministryAdminController.GetByFilter(new MinistryAdminFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        ministryAdminServiceMock.VerifyAll();
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetByFilter_WhenNoRecordsInDB_ReturnsNoContentResult()
    {
        // Arrange
        ministryAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()))
            .ReturnsAsync(new SearchResult<MinistryAdminDto> { TotalAmount = 0, Entities = new List<MinistryAdminDto>() });

        // Act
        var result = await ministryAdminController.GetByFilter(new MinistryAdminFilter()).ConfigureAwait(false);

        // Assert
        ministryAdminServiceMock.VerifyAll();
        Assert.IsInstanceOf<NoContentResult>(result);
    }
}

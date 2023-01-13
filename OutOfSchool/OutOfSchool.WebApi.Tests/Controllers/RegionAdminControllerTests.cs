using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class RegionAdminControllerTests
{
    private RegionAdminController regionAdminController;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private IMapper mapper;
    private RegionAdmin regionAdmin;
    private List<RegionAdmin> regionAdmins;
    private RegionAdminDto regionAdminDto;
    private List<RegionAdminDto> regionAdminDtos;
    private Mock<HttpContext> fakeHttpContext;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        regionAdminController =
            new RegionAdminController(regionAdminServiceMock.Object, new Mock<ILogger<RegionAdminController>>().Object);
        regionAdmin = AdminGenerator.GenerateRegionAdmin();
        regionAdmins = AdminGenerator.GenerateRegionAdmins(10);
        regionAdminDto = AdminGenerator.GenerateRegionAdminDto();
        regionAdminDtos = AdminGenerator.GenerateRegionAdminsDtos(10);
        fakeHttpContext = new Mock<HttpContext>();
        regionAdminController.ControllerContext.HttpContext = fakeHttpContext.Object;
    }

    [Test]
    public async Task GetById_WhenIdIsValid_ShouldReturnOkResultObject()
    {
        // Arrange
        regionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(regionAdminDto);

        // Act
        var result = await regionAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        regionAdminServiceMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.SameAs(regionAdminDto));
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_WhenNoRegionAdminWithSuchId_ReturnsNotFoundResult()
    {
        // Arrange
        regionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null as RegionAdminDto);

        // Act
        var result = await regionAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        regionAdminServiceMock.VerifyAll();
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetRegionAdmins_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        var expected = new SearchResult<RegionAdminDto>
        {
            TotalAmount = 10,
            Entities = regionAdmins.Select(x => mapper.Map<RegionAdminDto>(x)).ToList(),
        };

        regionAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<RegionAdminFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await regionAdminController.GetByFilter(new RegionAdminFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        regionAdminServiceMock.VerifyAll();
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetByFilter_WhenNoRecordsInDB_ReturnsNoContentResult()
    {
        // Arrange
        regionAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<RegionAdminFilter>()))
            .ReturnsAsync(new SearchResult<RegionAdminDto> { TotalAmount = 0, Entities = new List<RegionAdminDto>() });

        // Act
        var result = await regionAdminController.GetByFilter(new RegionAdminFilter()).ConfigureAwait(false);

        // Assert
        regionAdminServiceMock.VerifyAll();
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}

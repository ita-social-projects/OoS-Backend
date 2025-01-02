using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V2;
using System.Security.Claims;
using OutOfSchool.BusinessLogic.Models.Providers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using AutoMapper;
using OutOfSchool.Tests.Common;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class WorkshopDraftControllerTests
{
    private const int Ok = 200;
    private const int NoContent = 204;
    private const int Create = 201;
    private const int BadRequest = 400;
    private const int Forbidden = 403;

    private static WorkshopV2Dto workshopV2Dto;
    private static WorkshopDraftResultDto workshopDraftResultDto;
    private static ProviderDto provider;    

    private WorkshopDraftController controller;
    private Mock<IProviderService> providerServiceMoq;
    private Mock<IWorkshopDraftService> workshopDraftServiceMoq;
    private Mock<HttpContext> httpContextMoq;
    private IMapper mapper;

    private string userId;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        userId = "someUserId";
        httpContextMoq = new Mock<HttpContext>();
        httpContextMoq.Setup(x => x.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        httpContextMoq.Setup(x => x.User.IsInRole("provider"))
            .Returns(true);

        mapper = TestHelper.CreateMapperInstanceOfProfileType<WorkshopDraftMappingProfile>();

        provider = ProviderDtoGenerator.Generate();

        workshopV2Dto = WorkshopV2DtoGenerator.Generate();
        workshopV2Dto.Address = AddressDtoGenerator.Generate();
        workshopV2Dto.DateTimeRanges = DateTimeRangeDtoGenerator.Generate(5);
        workshopV2Dto.ProviderId = provider.Id;

        workshopDraftResultDto = new WorkshopDraftResultDto()
        {
            WorkshopDraft = mapper.Map<WorkshopDraftResponseDto>(mapper.Map<WorkshopDraft>(workshopV2Dto))
        };
    }

    [SetUp]
    public void Setup()
    {
        workshopDraftServiceMoq = new Mock<IWorkshopDraftService>();
        providerServiceMoq = new Mock<IProviderService>();

        controller = new WorkshopDraftController(
            providerServiceMoq.Object,
            workshopDraftServiceMoq.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
        };
    }

    #region CreateWorkshopDraft
    [Test]
    public async Task CreateWorkshopDraft_WhenModelIsValid_ShouldReturnCreatedAtActionResult()
    {
        // Arrange        
        workshopDraftServiceMoq.Setup(x => x.Create(workshopV2Dto))
            .ReturnsAsync(workshopDraftResultDto).Verifiable(Times.Once);
        providerServiceMoq.Setup(x => x.GetById(It.IsAny<Guid>()))            
            .ReturnsAsync(provider).Verifiable(Times.Once);

        // Act
        var result = await controller.Create(workshopV2Dto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert        
        providerServiceMoq.VerifyAll();
        workshopDraftServiceMoq.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(Create, result.StatusCode);
    }
    #endregion    
}

using System;
using System.Threading.Tasks;
using AutoMapper;
using NUnit.Framework;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.IntegrationTests.AdminsUpdateTests;

[TestFixture]
public class AdminsUpdate
{
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        this.mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(typeof(MappingProfile))));
    }

    [Test]
    public void Map_BaseUserDtoToRegionAdminBaseDto_ShouldNotMapInstitutionIdAndCatottgId()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        long catottgId = long.MaxValue;
        var regionAdminBaseDto = new RegionAdminBaseDto { CATOTTGId = catottgId, InstitutionId = institutionId };
        var baseUserDto = new BaseUserDto();

        // Act
        mapper.Map(baseUserDto, regionAdminBaseDto);

        // Assert
        Assert.That(regionAdminBaseDto.InstitutionId == institutionId, "Changing the InstitutionId is not allowed by business rules.");
        Assert.That(regionAdminBaseDto.CATOTTGId == catottgId, "Changing the CATOTTGId is not allowed by business rules.");
    }

    [Test]
    public void Map_BaseUserDtoToAreaAdminBaseDto_ShouldNotMapInstitutionIdAndCatottgId()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        long catottgId = long.MaxValue;
        var areaAdminBaseDto = new AreaAdminBaseDto { CATOTTGId = catottgId, InstitutionId = institutionId };
        var baseUserDto = new BaseUserDto();

        // Act
        mapper.Map(baseUserDto, areaAdminBaseDto);

        // Assert
        Assert.That(areaAdminBaseDto.InstitutionId == institutionId, "Changing the InstitutionId is not allowed by business rules.");
        Assert.That(areaAdminBaseDto.CATOTTGId == catottgId, "Changing the CATOTTGId is not allowed by business rules.");
    }

    [Test]
    public void Map_BaseUserDtoToMinistryAdminBaseDto_ShouldNotMapInstitutionId()
    {
        // Arrange
        var institutionId = Guid.NewGuid();
        var ministryAdminBaseDto = new MinistryAdminBaseDto { InstitutionId = institutionId };
        var baseUserDto = new BaseUserDto();

        // Act
        mapper.Map(baseUserDto, ministryAdminBaseDto);

        // Assert
        Assert.That(ministryAdminBaseDto.InstitutionId == institutionId, "Changing the InstitutionId is not allowed by business rules.");
    }
}

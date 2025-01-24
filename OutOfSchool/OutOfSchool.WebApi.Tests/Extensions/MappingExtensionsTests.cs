using AutoMapper;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Util;
using OutOfSchool.BusinessLogic.Util.Mapping;
using OutOfSchool.Common.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class MappingExtensionsTests
{
    // TODO: fix mapper configuration
    [Test]
    public void Mapping_MappingProfile_ConfigurationIsCorrect()
    {
        // act
        var configuration = new MapperConfiguration(cfg =>
            MappingExtensions.UseProfile<MappingProfile>(MappingExtensions.UseProfile<CommonProfile>(cfg)));

        // assert
        configuration.AssertConfigurationIsValid();
    }

    [Test]
    public void Mapping_ElasticProfile_ConfigurationIsCorrect()
    {
        // act
        var configuration = new MapperConfiguration(cfg =>
            MappingExtensions.UseProfile<ElasticProfile>(MappingExtensions.UseProfile<CommonProfile>(cfg)));

        // assert
        configuration.AssertConfigurationIsValid();
    }
    
    [Test]
    public void Mapping_ExternalExportMappingProfile_ConfigurationIsCorrect()
    {
        // act
        var configuration = new MapperConfiguration(cfg =>
            cfg.UseProfile<CommonProfile>().UseProfile<MappingProfile>().UseProfile<ExternalExportMappingProfile>());

        // assert
        configuration.AssertConfigurationIsValid();
    }
}
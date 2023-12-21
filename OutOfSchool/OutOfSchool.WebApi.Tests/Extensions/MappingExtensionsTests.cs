﻿using AutoMapper;
using NUnit.Framework;
using OutOfSchool.Common.Extensions;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.Mapping;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class MappingExtensionsTests
{
    // TODO: fix mapper configuration
    [Test]
    public void Mapping_MappingProfile_ConfigurationIsCorrect()
    {
        // act
        var configuration = new MapperConfiguration(cfg => MappingExtensions.UseProfile<MappingProfile>(MappingExtensions.UseProfile<CommonProfile>(cfg)));

        // assert
        configuration.AssertConfigurationIsValid();
    }

    [Test]
    public void Mapping_ElasticProfile_ConfigurationIsCorrect()
    {
        // act
        var configuration = new MapperConfiguration(cfg => MappingExtensions.UseProfile<ElasticProfile>(MappingExtensions.UseProfile<CommonProfile>(cfg)));

        // assert
        configuration.AssertConfigurationIsValid();
    }
}
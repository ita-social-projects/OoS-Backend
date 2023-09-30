﻿using AutoMapper;
using NUnit.Framework;
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
        // arrange
        var profile = new MappingProfile();

        // act
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));

        // assert
        configuration.AssertConfigurationIsValid();
    }

    [Test]
    public void Mapping_ElasticProfile_ConfigurationIsCorrect()
    {
        // arrange
        var profile = new ElasticProfile();

        // act
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));

        // assert
        configuration.AssertConfigurationIsValid();
    }
}
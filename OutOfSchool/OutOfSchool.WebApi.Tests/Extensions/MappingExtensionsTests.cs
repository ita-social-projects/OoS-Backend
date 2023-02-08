using System;
using System.Collections.Generic;
using System.Globalization;
using AutoMapper;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.ChatWorkshop;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ChatWorkshop;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.Workshop;
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
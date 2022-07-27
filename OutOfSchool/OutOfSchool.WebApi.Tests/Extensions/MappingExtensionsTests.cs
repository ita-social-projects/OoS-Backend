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

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class MappingExtensionsTests
{
    [Test]
    public void Mapping_WorkshopDTO_ToCardDto_IsCorrect()
    {
        // Arrange
        var workshopDto = new WorkshopDTO()
        {
            Id = Guid.NewGuid(),
            Title = "Title5",
            Phone = "1111111111",
            WorkshopDescriptionItems = new[]
            {
                new WorkshopDescriptionItemDto
                {
                    Id = Guid.NewGuid(),
                    SectionName = "test heading1",
                    Description = "test sentence of description1",
                },
            },
            Price = 5000,
            PayRate = OutOfSchool.Common.Enums.PayRateType.Classes,
            WithDisabilityOptions = true,
            ProviderTitle = "ProviderTitle",
            DisabilityOptionsDesc = "Desc5",
            Website = "website5",
            Instagram = "insta5",
            Facebook = "facebook5",
            Email = "email5@gmail.com",
            MaxAge = 10,
            MinAge = 4,
            CoverImageId = "image5",
            ProviderId = Guid.NewGuid(),
            DirectionId = 1,
            Direction = "Some title of direction",
            DepartmentId = 1,
            ClassId = 1,
            AddressId = 17,
            Address = new AddressDto
            {
                Id = 17,
                Region = "Region17",
                District = "District17",
                City = "City17",
                Street = "Street17",
                BuildingNumber = "BuildingNumber17",
                Latitude = 123.2355,
                Longitude = 23.1234,
            },
            Teachers = new List<TeacherDTO>()
            {
                new TeacherDTO
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Alex",
                    LastName = "Brown",
                    MiddleName = "SomeMiddleName",
                    Description = "Description",
                    CoverImageId = "Image",
                    DateOfBirth = DateTime.Parse("1990-01-01"),
                    WorkshopId = new Guid("3a217d24-4945-477b-9381-e9ee8dc1f338"),
                    Gender = Gender.Male,
                },
                new TeacherDTO
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Snow",
                    MiddleName = "SomeMiddleName",
                    Description = "Description",
                    CoverImageId = "Image",
                    DateOfBirth = DateTime.Parse("1990-01-01"),
                    WorkshopId = new Guid("3a217d24-4945-477b-9381-e9ee8dc1f338"),
                    Gender = Gender.Female,
                },
            },
            Keywords = new List<string>()
            {
                "dance",
                "twist",
            },
            Rating = 23.12314f,
        };

        // Act
        var result = workshopDto.ToCardDto();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<WorkshopCard>(result);
            Assert.AreEqual(workshopDto.Id, result.WorkshopId);
            Assert.AreEqual(workshopDto.Title, result.Title);
            Assert.AreEqual(workshopDto.Price, result.Price);
            Assert.AreEqual(workshopDto.PayRate, result.PayRate);
            Assert.AreEqual(workshopDto.ProviderId, result.ProviderId);
            Assert.AreEqual(workshopDto.ProviderTitle, result.ProviderTitle);
            Assert.AreEqual(workshopDto.MinAge, result.MinAge);
            Assert.AreEqual(workshopDto.MaxAge, result.MaxAge);
            Assert.AreEqual(workshopDto.CoverImageId, result.CoverImageId);
            Assert.AreEqual(workshopDto.DirectionId, result.DirectionId);
            Assert.AreEqual(workshopDto.Address?.Id, result.Address.Id);
            Assert.IsNotNull(result.Address);
            Assert.AreEqual(workshopDto.Address.Latitude, result.Address.Latitude);
            Assert.AreEqual(workshopDto.Rating, result.Rating);
        });
    }

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
    public void Mapping_ProviderDto_ToProvider_IsCorrect()
    {
        var providerDto = ProviderDtoGenerator.Generate();
        var provider = providerDto.ToDomain();

        EnsureProviderAndProviderDtoAreEqual(providerDto, provider);
    }

    [Test]
    public void Mapping_Provider_ToProviderDto_IsCorrect()
    {
        var provider = ProvidersGenerator.Generate();
        var providerDto = provider.ToModel();

        EnsureProviderAndProviderDtoAreEqual(providerDto, provider);
    }

    private static void EnsureProviderAndProviderDtoAreEqual(ProviderDto providerDto, Provider provider)
    {
        Assert.Multiple(() =>
        {
            Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
            Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
            Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
            Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
            Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
            Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
            Assert.That(providerDto.Id, Is.EqualTo(provider.Id));
            Assert.That(providerDto.FullTitle, Is.EqualTo(provider.FullTitle));
            Assert.That(providerDto.ShortTitle, Is.EqualTo(provider.ShortTitle));
            Assert.That(providerDto.Website, Is.EqualTo(provider.Website));
            Assert.That(providerDto.Email, Is.EqualTo(provider.Email));
            Assert.That(providerDto.Facebook, Is.EqualTo(provider.Facebook));
            Assert.That(providerDto.Instagram, Is.EqualTo(provider.Instagram));
            Assert.That(providerDto.EdrpouIpn, Is.EqualTo(provider.EdrpouIpn));
            Assert.That(providerDto.Director, Is.EqualTo(provider.Director));
            Assert.That(providerDto.DirectorDateOfBirth, Is.EqualTo(provider.DirectorDateOfBirth));
            Assert.That(providerDto.PhoneNumber, Is.EqualTo(provider.PhoneNumber));
            Assert.That(providerDto.Founder, Is.EqualTo(provider.Founder));
            Assert.That(providerDto.Ownership, Is.EqualTo(provider.Ownership));
            Assert.That(providerDto.Type, Is.EqualTo(provider.Type));
            Assert.That(providerDto.Status, Is.EqualTo(provider.Status));
            //Assert.That(providerDto.Rating, Is.EqualTo(provider.Rating));
            //Assert.That(providerDto.NumberOfRatings, Is.EqualTo(provider.NumberOfRatings));
            Assert.That(providerDto.UserId, Is.EqualTo(provider.UserId));
            // TODO: add address comparers
            //Assert.That(providerDto.LegalAddress, Is.EqualTo(provider.LegalAddress));
            //Assert.That(providerDto.ActualAddress, Is.EqualTo(provider.ActualAddress));
            Assert.That(providerDto.InstitutionStatusId, Is.EqualTo(provider.InstitutionStatusId));
        });
    }
}
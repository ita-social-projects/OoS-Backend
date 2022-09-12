using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Services;
using Xunit;

namespace OutOfSchool.WebApi.UnitTests.Services;
public class RatingServiceTests
{
    [Fact]
    public void Constructor_ShouldReturnObject()
    {
        // Arrange
        var ratingRepository = Substitute.For<IRatingRepository>();
        var workshopRepository = Substitute.For<IWorkshopRepository>();
        var providerRepository = Substitute.For<IProviderRepository>();
        var parentRepository = Substitute.For<IParentRepository>();
        var logger = Substitute.For<ILogger<RatingService>>();
        var localizer = Substitute.For<IStringLocalizer<SharedResource>>();
        var mapper = Substitute.For<IMapper>();

        // Act
        var service = new RatingService(ratingRepository, workshopRepository, providerRepository, parentRepository, logger, localizer, mapper);

        // Assert
        Assert.NotNull(service);
        Assert.IsType<RatingService>(service);
    }

    [Fact]
    public void Constructor_ShouldReturnArgumentNullException()
    {
        // Arrange
        var ratingRepository = Substitute.For<IRatingRepository>();
        var workshopRepository = Substitute.For<IWorkshopRepository>();
        var providerRepository = Substitute.For<IProviderRepository>();
        var parentRepository = Substitute.For<IParentRepository>();

        // Act

        // Assert
        Assert.Throws<ArgumentNullException>(() => new RatingService(ratingRepository, workshopRepository, providerRepository, parentRepository, null, null, null));
    }

    [Fact]
    public void GetAsync_ShouldReturn()
    {
        // Arrange
        var ratingRepository = Substitute.For<IRatingRepository>();
        var workshopRepository = Substitute.For<IWorkshopRepository>();
        var providerRepository = Substitute.For<IProviderRepository>();
        var parentRepository = Substitute.For<IParentRepository>();
        var logger = Substitute.For<ILogger<RatingService>>();
        var localizer = Substitute.For<IStringLocalizer<SharedResource>>();
        var mapper = Substitute.For<IMapper>();
        var service = new RatingService(ratingRepository, workshopRepository, providerRepository, parentRepository, logger, localizer, mapper);

        var rating = new Faker<Rating>()
            .CustomInstantiator(f => new Rating())
            .RuleFor(x => x.Id, f => f.Random.Number())
            .RuleFor(x => x.Rate, f => f.Random.Number(1, 5))
            .RuleFor(x => x.Type, f => f.PickRandom<RatingType>())
            .RuleFor(x => x.EntityId, f => f.Random.Guid())
            .RuleFor(x => x.ParentId, f => f.Random.Guid())
            .RuleFor(x => x.CreationTime, f => f.Date.PastOffset());

        var data = rating.GenerateBetween(1, 5).AsQueryable();

        var mockSet = Substitute.For<DbSet<Rating>, IQueryable<Rating>>();
        var mockEnumerableSet = Substitute.For<DbSet<Rating>, IEnumerable<Rating>>();

        ((IQueryable<Rating>)mockSet).Provider.Returns(data.Provider);
        ((IQueryable<Rating>)mockSet).Expression.Returns(data.Expression);
        ((IQueryable<Rating>)mockSet).ElementType.Returns(data.ElementType);
        ((IQueryable<Rating>)mockSet).GetEnumerator().Returns(data.GetEnumerator());
        ((IEnumerable<Rating>)mockEnumerableSet).GetEnumerator().Returns(data.GetEnumerator());

        // Act
        ratingRepository.Get().Returns(mockEnumerableSet);
        var res = service.GetAsync(new Models.OffsetFilter());

        // Assert

    }


}

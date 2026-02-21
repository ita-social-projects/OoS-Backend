using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.Tests;

public static class UnitTestHelper
{
    public static DbContextOptions<OutOfSchoolDbContext> GetUnitTestDbOptions()
    {
        var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).EnableSensitiveDataLogging()
            .UseLazyLoadingProxies()
            .Options;

        using (var context = new TestOutOfSchoolDbContext(options))
        {
            SeedData(context);
        }

        return options;
    }

    public static void SeedData(TestOutOfSchoolDbContext context)
    {
        var socialGroups = new List<SocialGroup>
        {
            new SocialGroup { Id = 1, Name = "sg1" },
            new SocialGroup { Id = 2, Name = "sg2" },
            new SocialGroup { Id = 3, Name = "sg3" },
        };

        context.SocialGroups.AddRange(socialGroups);

        var parents = new List<Parent>
        {
            new Parent { Id = Guid.NewGuid(), UserId = Guid.NewGuid().ToString(), Gender = Gender.Male, DateOfBirth = DateTime.Today},
            new Parent { Id = Guid.NewGuid(), UserId = Guid.NewGuid().ToString(), Gender = Gender.Female, DateOfBirth = DateTime.Today },
            new Parent { Id = Guid.NewGuid(), UserId = Guid.NewGuid().ToString(), Gender = Gender.Male, DateOfBirth = DateTime.Today },
        };

        context.Parents.AddRange(parents);

        var children = ChildGenerator.Generate(3);
        children.ForEach(c => c.WithSocial(socialGroups.RandomItem())
            .WithParent(parents.RandomItem()));

        context.Children.AddRange(children);

        context.Directions.Add(new Direction { Id = 1, Title = "c1" });
        context.Directions.Add(new Direction { Id = 2, Title = "c2" });
        context.Directions.Add(new Direction { Id = 3, Title = "c3" });

        var workshops = new List<Workshop>
        {
            new Workshop { Id = Guid.NewGuid(), Title = "w1" },
            new Workshop { Id = Guid.NewGuid(), Title = "w2" },
            new Workshop { Id = Guid.NewGuid(), Title = "w3" },
        };

        context.Workshops.AddRange(workshops);

        context.Applications.Add(new Application() { Id = Guid.NewGuid(), ChildId = children[0].Id, Status = ApplicationStatus.Pending, WorkshopId = workshops[0].Id, ParentId = parents[0].Id, CreationTime = new DateTime(2021, 7, 9) });
        context.Applications.Add(new Application() { Id = Guid.NewGuid(), ChildId = children[1].Id, Status = ApplicationStatus.Pending, WorkshopId = workshops[0].Id, ParentId = parents[0].Id, CreationTime = new DateTime(2021, 7, 9) });
        context.Applications.Add(new Application() { Id = Guid.NewGuid(), ChildId = children[2].Id, Status = ApplicationStatus.Pending, WorkshopId = workshops[2].Id, ParentId = parents[0].Id, CreationTime = new DateTime(2021, 7, 9) });

        context.SaveChanges();
    }
}
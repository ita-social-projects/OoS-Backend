using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests
{
    public static class UnitTestHelper
    {
        public static DbContextOptions<OutOfSchoolDbContext> GetUnitTestDbOptions()
        {
            var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseLazyLoadingProxies()
                .Options;

            using (var context = new OutOfSchoolDbContext(options))
            {
                SeedData(context);
            }

            return options;
        }

        public static void SeedData(OutOfSchoolDbContext context)
        {
            context.SocialGroups.Add(new SocialGroup { Id = 1, Name = "sg1" });
            context.SocialGroups.Add(new SocialGroup { Id = 2, Name = "sg2" });
            context.SocialGroups.Add(new SocialGroup { Id = 3, Name = "sg3" });
            context.Parents.Add(new Parent { Id = 1 });
            context.Parents.Add(new Parent { Id = 2 });
            context.Parents.Add(new Parent { Id = 3 });
            context.Children.Add(new Child { Id = 1, FirstName = "fn1", LastName = "ln1", MiddleName = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 });
            context.Children.Add(new Child { Id = 2, FirstName = "fn2", LastName = "ln2", MiddleName = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = 2, SocialGroupId = 1 });
            context.Children.Add(new Child { Id = 3, FirstName = "fn3", LastName = "ln3", MiddleName = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 });

            context.Directions.Add(new Direction { Id = 1, Title = "c1" });
            context.Directions.Add(new Direction { Id = 2, Title = "c2" });
            context.Directions.Add(new Direction { Id = 3, Title = "c3" });

            context.Departments.Add(new Department { Id = 1, Title = "sc1", DirectionId = 1 });
            context.Departments.Add(new Department { Id = 2, Title = "sc2", DirectionId = 3 });

            context.Classes.Add(new Class { Id = 1, Title = "ssc1", DepartmentId = 1 });
            context.Classes.Add(new Class { Id = 2, Title = "ssc2", DepartmentId = 1 });
            context.Classes.Add(new Class { Id = 3, Title = "ssc3", DepartmentId = 2 });

            context.Workshops.Add(new Workshop { Id = 1, Title = "w1", DirectionId = 1 });
            context.Workshops.Add(new Workshop { Id = 2, Title = "w2", DirectionId = 1 });
            context.Workshops.Add(new Workshop { Id = 3, Title = "w3", DirectionId = 3 });

            context.Applications.Add(new Application() { Id = Guid.NewGuid(), ChildId = 1, Status = ApplicationStatus.Pending, WorkshopId = 1, ParentId = 1, CreationTime = new DateTime(2021, 7, 9) });
            context.Applications.Add(new Application() { Id = Guid.NewGuid(), ChildId = 1, Status = ApplicationStatus.Pending, WorkshopId = 1, ParentId = 1, CreationTime = new DateTime(2021, 7, 9) });
            context.Applications.Add(new Application() { Id = Guid.NewGuid(), ChildId = 1, Status = ApplicationStatus.Pending, WorkshopId = 3, ParentId = 1, CreationTime = new DateTime(2021, 7, 9) });

            context.SaveChanges();
        }
    }
}
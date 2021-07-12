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
            context.Parents.Add(new Parent { Id = 1, });
            context.Parents.Add(new Parent { Id = 2, });
            context.Parents.Add(new Parent { Id = 3, });
            context.Children.Add(new Child { Id = 1, FirstName = "fn1", LastName = "ln1", MiddleName = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 });
            context.Children.Add(new Child { Id = 2, FirstName = "fn2", LastName = "ln2", MiddleName = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = 2, SocialGroupId = 1 });
            context.Children.Add(new Child { Id = 3, FirstName = "fn3", LastName = "ln3", MiddleName = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 });

            context.Workshops.Add(new Workshop { Id = 1, Title = "w1" });
            context.Workshops.Add(new Workshop { Id = 2, Title = "w2" });

            context.Applications.Add(new Application() { Id = 1, ChildId = 1, Status = ApplicationStatus.Pending, WorkshopId = 1, ParentId = 1, Child = context.Children.Find(1L), Parent = context.Parents.Find(1L), Workshop = context.Workshops.Find(1L) });
            context.Applications.Add(new Application() { Id = 3, ChildId = 1, Status = ApplicationStatus.Pending, WorkshopId = 1, ParentId = 1, Child = context.Children.Find(1L), Parent = context.Parents.Find(1L), Workshop = context.Workshops.Find(1L) });

            context.SaveChanges();
        }
    }
}
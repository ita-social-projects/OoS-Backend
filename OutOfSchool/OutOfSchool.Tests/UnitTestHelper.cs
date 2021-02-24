using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
            context.SocialGroups.Add(new SocialGroup { SocialGroupId = 1, Name = "sg1" });
            context.SocialGroups.Add(new SocialGroup { SocialGroupId = 2, Name = "sg2" });
            context.SocialGroups.Add(new SocialGroup { SocialGroupId = 3, Name = "sg3" });
            context.Parents.Add(new Parent { ParentId = 1, FirstName = "fn1", LastName = "ln1" });
            context.Parents.Add(new Parent { ParentId = 2, FirstName = "fn2", LastName = "ln2" });
            context.Parents.Add(new Parent { ParentId = 3, FirstName = "fn3", LastName = "ln3" });
            context.Children.Add(new Child { ChildId = 1, FirstName = "fn1", LastName = "ln1", MiddleName = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 });
            context.Children.Add(new Child { ChildId = 2, FirstName = "fn2", LastName = "ln2", MiddleName = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = 2, SocialGroupId = 1 });
            context.Children.Add(new Child { ChildId = 3, FirstName = "fn3", LastName = "ln3", MiddleName = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 });
            context.SaveChanges();
        }
    }
}

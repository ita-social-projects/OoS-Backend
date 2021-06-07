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
            context.Parents.Add(new Parent { Id = 1, FirstName = "fn1", LastName = "ln1" });
            context.Parents.Add(new Parent { Id = 2, FirstName = "fn2", LastName = "ln2" });
            context.Parents.Add(new Parent { Id = 3, FirstName = "fn3", LastName = "ln3" });
            context.Children.Add(new Child { Id = 1, FirstName = "fn1", LastName = "ln1", Patronymic = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 });
            context.Children.Add(new Child { Id = 2, FirstName = "fn2", LastName = "ln2", Patronymic = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = 2, SocialGroupId = 1 });
            context.Children.Add(new Child { Id = 3, FirstName = "fn3", LastName = "ln3", Patronymic = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 });

            context.Categories.Add(new Category { Id = 1, Title = "c1" });
            context.Categories.Add(new Category { Id = 2, Title = "c2" });
            context.Categories.Add(new Category { Id = 3, Title = "c3" });

            context.Subcategories.Add(new Subcategory { Id = 1, Title = "sc1", CategoryId = 1 });
            context.Subcategories.Add(new Subcategory { Id = 2, Title = "sc2", CategoryId = 3 });

            context.Subsubcategories.Add(new Subsubcategory { Id = 1, Title = "ssc1", SubcategoryId = 1 });
            context.Subsubcategories.Add(new Subsubcategory { Id = 2, Title = "ssc2", SubcategoryId = 1 });
            context.Subsubcategories.Add(new Subsubcategory { Id = 3, Title = "ssc3", SubcategoryId = 2 });

            context.Workshops.Add(new Workshop { Id = 1, Title = "w1", SubsubcategoryId = 1 });
            context.Workshops.Add(new Workshop { Id = 2, Title = "w2", SubsubcategoryId = 2 });
            context.Workshops.Add(new Workshop { Id = 3, Title = "w3", SubsubcategoryId = 3 });

            context.Applications.Add(new Application() { Id = 1, ChildId = 1, Status = ApplicationStatus.Pending, WorkshopId = 1, UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6" });
            context.Applications.Add(new Application() { Id = 3, ChildId = 1, Status = ApplicationStatus.Pending, WorkshopId = 1, UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6" });
            context.Applications.Add(new Application() { Id = 10, ChildId = 1, Status = ApplicationStatus.Pending, WorkshopId = 3, UserId = "de909f35-5eb7-4b7a-bda8-40a5bfdaEEa6" });

            context.SaveChanges();
        }
    }
}
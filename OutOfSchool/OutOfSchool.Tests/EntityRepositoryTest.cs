using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Models;
using System.Linq;
using OutOfSchool.Services.Enums;
using Moq;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services;
using OutOfSchool.Services.Repository;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.Tests
{
    [TestFixture]
    public class EntityRepositoryTest
    {
        [Test]
        [TestCase(1)]
        public void GetAllWIthDetails_FilterWithId_ReturnSingleEntity(long id)
        {
            var myDatabaseName = "mydatabase_" + DateTime.Now.ToFileTimeUtc();
            var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
             .UseInMemoryDatabase(databaseName: myDatabaseName)
             .Options;


            using (var context = new OutOfSchoolDbContext(options))
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


                var repository = new EntityRepository<Child>(context);
                Expression<Func<Child, bool>> filter = child => child.ChildId == id;
                //Act

                var child = repository.GetAllWIthDetails(filter).Result;

                //Assert
                Assert.AreEqual(1, child.Count());
            }
        }
    }
}

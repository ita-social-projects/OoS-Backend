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
        [TestCase(2)]
        [TestCase(3)]
        public void GetAllWIthDetails_FilterWithId_ReturnSingleEntity(long id)
        {

            using (var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions()))
            {

                var repository = new EntityRepository<Child>(context);
                Expression<Func<Child, bool>> filter = child => child.Id == id;
                //Act

                var child = repository.GetAllWIthDetails(filter).Result;

                //Assert
                Assert.AreEqual(1, child.Count());
            }
        }

        [Test]
        public void Create_NewEntity_AddNewEntityToDatabase()
        {
            using (var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions()))
            {

                var repository = new EntityRepository<Child>(context);
                Child child = new Child {FirstName = "fn4", LastName = "ln4", MiddleName = "mn4", DateOfBirth = new DateTime(2006, 4, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 }; 
                //Act

                var child1 = repository.Create(child).Result;
                var children = repository.GetAll();

                //Assert
                Assert.AreEqual(4, children.Result.Count());
            }
        }

        [Test]
        public void Delete_DeleteEntity_DeleteFromDatabase()
        {
            using (var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions()))
            {

                var repository = new EntityRepository<SocialGroup>(context);
                SocialGroup socialGroup = new SocialGroup { Id = 1, Name = "sg1" };
                //Act

                repository.Delete(socialGroup);
                var socialGroups = repository.GetAll();

                //Assert
                Assert.AreEqual(2, socialGroups.Result.Count());

            }
        }

        [Test]
        public void GetAll_ReturnAllValues()
        {
            using (var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions()))
            {

                var repository = new EntityRepository<SocialGroup>(context);
           
                //Act
 
                var socialGroups = repository.GetAll();

                //Assert
                Assert.AreEqual(3, socialGroups.Result.Count());

            }
        }

        [Test]
        public void Update_UpatedInfo_UpdateEntityInDatabase()
        {
            using (var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions()))
            {

                var repository = new EntityRepository<SocialGroup>(context);

                //Act
                SocialGroup socialGroup = new SocialGroup { Id = 2, Name = "sg22" };
                var socialGroup1 = repository.Update(socialGroup).Result;

                //Assert
                Assert.AreEqual(2, socialGroup1.Id);
                Assert.AreEqual("sg22", socialGroup1.Name);

            }
        }
    }
}

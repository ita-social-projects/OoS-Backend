using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base;
using OutOfSchool.Tests;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.WebApi.Tests;

[Obsolete("These tests check if EF Core is working, no real value")]
[TestFixture]
public class EntityRepositoryTest
{
    [Test]
    public void GetById_Id_ReturnEntity()
    {
        using var context = new TestOutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
        {
            var repository = new EntityRepositorySoftDeleted<long, SocialGroup>(context);

            // Act
            var group = repository.GetById(1).Result;

            // Assert
            Assert.NotNull(group);
        }
    }

    //[Test]
    //public async Task GetAllWIthDetails_FilterWithIdAndIncludedProperty_ReturnSingleEntity()
    //{
    //    using var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
    //    {
    //        var repository = new EntityRepository<Child>(context);
    //        Expression<Func<Child, bool>> filter = child => child.Id == 1;

    //        // Act
    //        var child = await repository.GetByFilter(filter, "SocialGroup").ConfigureAwait(false);

    //        // Assert
    //        Assert.AreEqual(1, child.Count());
    //        Assert.AreEqual("sg2", child.Where(a => a.Id == 1).Select(a => a.SocialGroup.Name).FirstOrDefault());
    //    }
    //}

    //[Test]
    //public void GetAllWithDetails_IncludeProperty_ReturnListOfElementsWithIncludedProperty()
    //{
    //    using var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
    //    {
    //        var repository = new EntityRepository<Child>(context);

    //        // Act
    //        var children = repository.GetAllWithDetails("Parent").Result;

    //        // Assert
    //        Assert.AreEqual(3, children.Count());
    //        Assert.AreEqual(1, children.Where(a => a.Id == 1).Select(a => a.Parent.Id).FirstOrDefault());
    //    }
    //}

    //[Test]
    //public void GetByFilterNoTracking_FilterWithIdAndIncludedProperty_ReturnSingleEntity()
    //{
    //    using var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
    //    {
    //        var repository = new EntityRepository<Child>(context);
    //        Expression<Func<Child, bool>> filter = child => child.Id == 1;

    //        // Act
    //        var child = repository.GetByFilterNoTracking(filter, "SocialGroup");

    //        // Assert
    //        Assert.AreEqual(1, child.Count());
    //        Assert.AreEqual("sg2", child.Where(a => a.Id == 1).Select(a => a.SocialGroup.Name).FirstOrDefault());
    //    }
    //}

    //[Test]
    //public void Create_NewEntity_AddNewEntityToDatabase()
    //{
    //    using var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
    //    {
    //        var repository = new EntityRepository<Child>(context);
    //        Child child = new Child { FirstName = "fn4", LastName = "ln4", MiddleName = "mn4", DateOfBirth = new DateTime(2006, 4, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 };

    //        // Act
    //        var child1 = repository.Create(child).Result;
    //        var children = repository.GetAll();

    //        // Assert
    //        Assert.AreEqual(4, children.Result.Count());
    //        Assert.AreEqual(child.LastName, child1.LastName);
    //    }
    //}

    [Test]
    public async Task Delete_DeleteEntity_DeleteFromDatabaseAsync()
    {
        using var context = new TestOutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
        {
            var repository = new EntityRepositorySoftDeleted<long, SocialGroup>(context);
            SocialGroup socialGroup = new SocialGroup { Id = 1, Name = "sg1" };

            // Act
            await repository.Delete(socialGroup);
            var socialGroups = repository.GetAll();

            // Assert
            Assert.AreEqual(2, socialGroups.Result.Count());
        }
    }

    [Test]
    public void GetAll_ReturnAllValues()
    {
        using var context = new TestOutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
        {
            var repository = new EntityRepositorySoftDeleted<long, SocialGroup>(context);

            // Act
            var socialGroups = repository.GetAll();

            // Assert
            Assert.AreEqual(3, socialGroups.Result.Count());
        }
    }

    [Test]
    public void Update_UpatedInfo_UpdateEntityInDatabase()
    {
        using var context = new TestOutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions());
        {
            var repository = new EntityRepositorySoftDeleted<long, SocialGroup>(context);

            // Act
            SocialGroup socialGroup = new SocialGroup { Id = 2, Name = "sg22" };
            var socialGroup1 = repository.Update(socialGroup).Result;

            // Assert
            Assert.AreEqual(2, socialGroup1.Id);
            Assert.AreEqual("sg22", socialGroup1.Name);
        }
    }
}
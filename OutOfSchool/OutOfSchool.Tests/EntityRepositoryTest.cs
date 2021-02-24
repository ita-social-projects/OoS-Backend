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

            using (var context = new OutOfSchoolDbContext(UnitTestHelper.GetUnitTestDbOptions()))
            {

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

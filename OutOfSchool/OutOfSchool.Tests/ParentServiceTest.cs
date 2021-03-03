using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Mapping.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Tests
{
        [TestFixture]
        public class ParentServiceTest
        {
            private OutOfSchoolDbContext _context;
            private IEntityRepository<Parent> _entityRepository;
            private IParentService _service;

            [SetUp]
            public void SetUp()
            {
                var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(databaseName: "Test").Options;
                _context = new OutOfSchoolDbContext(options);
                _entityRepository = new EntityRepository<Parent>(_context);
                _service = new ParentService(_entityRepository);
            }

            [Test]
            public async Task Create_Parent_ReturnsCreatedParent()
            {
                //Arrange

                var newParent = new ParentDTO { FirstName = "Test4", MiddleName = "Test", LastName = "Test" };

                //Act

                var parent = await _service.Create(newParent).ConfigureAwait(false);

                //Assert

                Assert.That(newParent.LastName, Is.EqualTo(parent.LastName), "Last names are equal");
            }

            [Test]
            public async Task GetAll_Parents_ReturnsSameAmountParents()
            {
                //Arrange

                var parent1 = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "Test" };
                var parent2 = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "Test" };
                await _service.Create(parent1);
                await _service.Create(parent2);

                //Act

                var list = await _service.GetAll();

                //Assert

                Assert.AreEqual(_context.Parents.Count(), list.Count());
            }

            [Test]
            public async Task Get_ParentById_ReturnsParentWithSameId()
            {
                //Arrange

                var parent = new ParentDTO { FirstName = "Yehor", MiddleName = "Test", LastName = "Test" };
                await _service.Create(parent);
                long id = _context.Parents.Where(x => x.FirstName == "Yehor").FirstOrDefault().Id;
                ParentDTO parentFromContext = _context.Parents.Where(x => x.Id == id).FirstOrDefault().ToModel();

                //Act

                ParentDTO parentGetByID = await this._service.GetById(id);

                //Assert

                Assert.That(parentFromContext.Id, Is.EqualTo(parentGetByID.Id), "Ids are equal");
                Assert.That(parentFromContext.LastName, Is.EqualTo(parentGetByID.LastName), "Last names are equal");
            }

            [Test]
            public void Delete_Parent_DeletedFromDatabase()
            {
                //Arrange

                var parent = new ParentDTO { FirstName = "Delete", MiddleName = "Test", LastName = "Test" };
                _service.Create(parent);
                long id = _context.Parents.Where(x => x.FirstName == parent.FirstName).FirstOrDefault().Id;

                //Act

                _service.Delete(id);

                //Assert
                Assert.AreEqual(_context.Parents.Where(x => x.Id == id).FirstOrDefault(), null);
            }

            [Test]
            public void Update_Parent_UpdatedParentInDb()
            {
                //Arrange

                var parent = new ParentDTO { FirstName = "Update", MiddleName = "Test", LastName = "Test" };
                _service.Create(parent);
                int id = (int)_context.Parents.Where(x => x.FirstName == "Update").FirstOrDefault().Id;
                parent.Id = id;
                parent.FirstName = "Update2";

                //Act

                _service.Update(parent);

                 //Assert

                Assert.AreNotEqual(_context.Parents.Where(x => x.FirstName == parent.FirstName), null);
            }


            [Test]
            public void Delete_Parent_ReturnsArgumentException()
            {
                //Arrange 

                var parent1 = new ParentDTO { FirstName = "Update", MiddleName = "Test", LastName = "Test" };
                _service.Create(parent1);
                int id = (int)_context.Parents.Select(x => x.Id).Max() + 10;

                //Act and Assert

                Assert.That(() => _service.Delete(id), Throws.ArgumentException);
            }

            [Test]
            public void GetById_Parent_ReturnsArgumentException()
            {
                //Arrange

                var parent1 = new ParentDTO { FirstName = "Update", MiddleName = "Test", LastName = "Test" };
                _service.Create(parent1);
                int id = (int)_context.Parents.Select(x => x.Id).Max() + 10;

                //Act and Assert

                Assert.That(() => _service.GetById(id), Throws.ArgumentException);
            }


             [TestCaseSource(nameof(ParentModelsData))]
            public void Create_Parent_ReturnsArgumentException(ParentDTO parent)
            {
                //Act and Assert

                Assert.That(() => _service.Create(parent), Throws.ArgumentException);
            }

            [TestCaseSource(nameof(ParentModelsData))]
            public void Update_Parent_ReturnsArgumentException(ParentDTO parent)
            {
                //Arrange

                _service.Create(new ParentDTO() { FirstName = "Test25", MiddleName = "Test", LastName = "Test" });
                 if (parent != null) parent.Id = _context.Parents.First().Id;

                //Act and Assert

                Assert.That(() => _service.Update(parent), Throws.ArgumentException);
            }

            public static IEnumerable<TestCaseData> ParentModelsData=>
                new List<TestCaseData>()
                {
                    new TestCaseData(new ParentDTO() { FirstName = "Test", MiddleName = "Test", LastName = "" }),
                    new TestCaseData(new ParentDTO() { FirstName = "Test", MiddleName = "", LastName = "Test" }),
                    new TestCaseData(new ParentDTO() { FirstName = "", MiddleName = "Test", LastName = "Test" }),
                    new TestCaseData(null),
                };
        }
}

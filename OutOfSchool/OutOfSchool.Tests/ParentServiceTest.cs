using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Implementation;
using OutOfSchool.WebApi.Services.Interfaces;
using OutOfSchool.WebApi.Services.Mapping;
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
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>().UseInMemoryDatabase(databaseName: "Test").Options;
            _context = new OutOfSchoolDbContext(options);
            _entityRepository = new EntityRepository<Parent>(_context);
            var parentMapper = new MapperConfiguration(x => x.AddProfile(new ParentMapperProfile())).CreateMapper();
            _mapper = parentMapper;
            _service = new ParentService(_entityRepository, parentMapper);
        }

        [Test]
        public void TestCreate()
        {
            var parent1 = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "Test" };
            var parent2 = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "Test" };
            var parent3 = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "Test" };
            List<ParentDTO> parents = new List<ParentDTO>();
            parents.Add(parent1);
            parents.Add(parent2);
            parents.Add(parent3);
            _service.Create(parent1);
            _service.Create(parent2);
            _service.Create(parent3);
            Assert.AreEqual(_context.Parents.Where(x => x.FirstName == "Test").ToList().Count(), parents.Count);
        }

        [Test]
        public void TestGetAll()
        {
            var parent1 = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "Test" };
            var parent2 = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "Test" };
            _service.Create(parent1);
            _service.Create(parent2);
            Assert.AreEqual(_context.Parents.Count(), _service.GetAll().Count());
        }

        [Test]
        public async Task TestGetById()
        {
            var parent1 = new ParentDTO { FirstName = "Yehor", MiddleName = "Test", LastName = "Test" };
            _service.Create(parent1);
            int id = (int)_context.Parents.Where(x => x.FirstName == "Yehor").FirstOrDefault()?.ParentId;
            Parent parent = _context.Parents.Where(x => x.ParentId == id).FirstOrDefault();
            ParentDTO tmp1 = _mapper.Map<Parent, ParentDTO>(parent);
            ParentDTO tmp2 = await this._service.GetById(id);
            bool res = (tmp1.FirstName == tmp2.FirstName);
            Assert.IsTrue(res);
        }

        [Test]
        public void TestDelete()
        {
            var parent1 = new ParentDTO { FirstName = "Delete", MiddleName = "Test", LastName = "Test" };
            _service.Create(parent1);
            int id = (int)_context.Parents.Where(x => x.FirstName == parent1.FirstName).FirstOrDefault().ParentId;
            _service.Delete(id);
            Assert.AreEqual(_context.Parents.Where(x => x.ParentId == id).FirstOrDefault(), null);
        }

        [Test]
        public void TestUpdate()
        {
            var parent1 = new ParentDTO { FirstName = "Update", MiddleName = "Test", LastName = "Test" };
            _service.Create(parent1);
            int id = (int)_context.Parents.Where(x => x.FirstName == "Update").FirstOrDefault().ParentId;
            parent1.Id = id;
            parent1.FirstName = "Update2";
            _service.Update(parent1);
            Assert.AreNotEqual(_context.Parents.Where(x => x.FirstName == parent1.FirstName), null);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void TestCreateExceptions(int flag)
        {
            ParentDTO parent = null;
            switch (flag)
            {
                case 1:
                    {
                        parent = null;
                        break;
                    }
                case 2:
                    {
                    parent = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "" };
                    break;
                    }
                case 3:
                    {
                        parent = new ParentDTO { FirstName = "Test", MiddleName = "", LastName = "Test" };
                        break;
                    }
                case 4:
                    {
                        parent = new ParentDTO { FirstName = "", MiddleName = "Test", LastName = "Test" };
                        break;
                    }
                default:
                    break;
            }
            Assert.That(() => _service.Create(parent), Throws.ArgumentException);
        }

        [Test]
        public void TestDeleteException()
        {
            var parent1 = new ParentDTO { FirstName = "Update", MiddleName = "Test", LastName = "Test" };
            _service.Create(parent1);
            int id = (int)_context.Parents.Select(x => x.ParentId).Max() + 10;
            Assert.That(() => _service.Delete(id), Throws.ArgumentException);
        }

        [Test]
        public void TestGetByIdException()
        {
            var parent1 = new ParentDTO { FirstName = "Update", MiddleName = "Test", LastName = "Test" };
            _service.Create(parent1);
            int id = (int)_context.Parents.Select(x => x.ParentId).Max() + 10;
            Assert.That(() => _service.GetById(id), Throws.ArgumentException);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void TestUpdateExceptions(int flag)
        {
            ParentDTO parent = null;
            switch (flag)
            {
                case 1:
                    {
                        parent = null;
                        break;
                    }
                case 2:
                    {
                        parent = new ParentDTO { FirstName = "Test", MiddleName = "Test", LastName = "" };
                        break;
                    }
                case 3:
                    {
                        parent = new ParentDTO { FirstName = "Test", MiddleName = "", LastName = "Test" };
                        break;
                    }
                case 4:
                    {
                        parent = new ParentDTO { FirstName = "", MiddleName = "Test", LastName = "Test" };
                        break;
                    }
                case 5:
                    {
                        var parent1 = new ParentDTO { FirstName = "Update", MiddleName = "Test", LastName = "Test" };
                        _service.Create(parent1);
                        int id = (int)_context.Parents.Select(x => x.ParentId).Max() + 10;
                        parent = new ParentDTO { Id = id, FirstName = "Test", MiddleName = "Test", LastName = "Test" };
                        break;
                    }
                default:
                    break;
            }
            Assert.That(() => _service.Update(parent), Throws.ArgumentException);
        }
    }
}

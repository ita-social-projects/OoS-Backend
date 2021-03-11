using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services
{
    public class ChildServiceTests
    {
        [Test]
        public void ChildService_GetAll_ReturnsChildrenModels()
        {
            var expected = GetTestChildDTO().ToList();
            var mockRepository = new Mock<IEntityRepository<Child>>();
            mockRepository.Setup(m => m.GetAll()).Returns(Task.FromResult(GetTestChildEntities()));
            IChildService childService = new ChildService(mockRepository.Object);

            var actual = childService.GetAll().Result.ToList();

            for (int i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(expected[i].Id, actual[i].Id);
                Assert.AreEqual(expected[i].FirstName, actual[i].FirstName);
                Assert.AreEqual(expected[i].LastName, actual[i].LastName);
                Assert.AreEqual(expected[i].Patronymic, actual[i].Patronymic);
            }
        }

        private IEnumerable<ChildDTO> GetTestChildDTO()
        {
            return new List<ChildDTO>()
            {
                new ChildDTO {Id = 1, FirstName = "fn1", LastName = "ln1", Patronymic = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 },
                new ChildDTO { Id = 2, FirstName = "fn2", LastName = "ln2", Patronymic = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = 2, SocialGroupId = 1},
                new ChildDTO {Id = 3, FirstName = "fn3", LastName = "ln3", Patronymic = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 }
            };
        }

        private IEnumerable<Child> GetTestChildEntities()
        {
            return new List<Child>()
            {
                new Child {Id = 1, FirstName = "fn1", LastName = "ln1", Patronymic = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 },
                new Child { Id = 2, FirstName = "fn2", LastName = "ln2", Patronymic = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = 2, SocialGroupId = 1},
                new Child {Id = 3, FirstName = "fn3", LastName = "ln3", Patronymic = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 }
            };
        }
    }
}

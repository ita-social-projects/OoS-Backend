using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace OutOfSchool.WebApi.Tests.Controllers
{
    [TestFixture]
    public class ChildControllerTests
    {
        private ChildController controller;
        private Mock<IChildService> service;
        private Mock<IEntityRepository<Child>> repo;

        private IEnumerable<ChildDTO> children;
        private ChildDTO child;

        [SetUp]
        public void Setup()
        {
            repo = new Mock<IEntityRepository<Child>>();
            service = new Mock<IChildService>();
            controller = new ChildController(service.Object);

            children = FakeChildren();
            child = FakeChild();
        }

        private ChildDTO FakeChild()
        {
            return new ChildDTO()
            {
                Id = 1,
                FirstName = "fn1",
                LastName = "ln1",
                Patronymic = "mn1",
                DateOfBirth = new DateTime(2003, 11, 9),
                Gender = Gender.Male,
                ParentId = 1,
                SocialGroupId = 2
            };
        }

        private IEnumerable<ChildDTO> FakeChildren()
        {
            return new List<ChildDTO>()
            {
                new ChildDTO()
                {
                    Id = 1,
                    FirstName = "fn1",
                    LastName = "ln1",
                    Patronymic = "mn1",
                    DateOfBirth = new DateTime(2003, 11, 9),
                    Gender = Gender.Male,
                    ParentId = 1,
                    SocialGroupId = 2,
                },
                new ChildDTO()
                {
                    Id = 2,
                    FirstName = "fn2",
                    LastName = "ln2",
                    Patronymic = "mn2",
                    DateOfBirth = new DateTime(2004, 11, 8),
                    Gender = Gender.Female,
                    ParentId = 2,
                    SocialGroupId = 1,
                },
                new ChildDTO()
                {
                    Id = 3,
                    FirstName = "fn3",
                    LastName = "ln3",
                    Patronymic = "mn3",
                    DateOfBirth = new DateTime(2006, 11, 2),
                    Gender = Gender.Male,
                    ParentId = 1,
                    SocialGroupId = 1,
                },
            };
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Services;

//public class ChildServiceTests
//{
//    private readonly Child createdChild = new Child { Id = 14, FirstName = "fn4", LastName = "ln4", MiddleName = "mn4", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 };
//    private readonly ChildDto createdChildDTO = new ChildDto { Id = 14, FirstName = "fn4", LastName = "ln4", MiddleName = "mn4", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 };

//    private Mock<ILogger<ChildService>> logger;
//    private Mock<IStringLocalizer<SharedResource>> localizer;
//    private Mock<IEntityRepository<Child>> mockRepository;

//    [SetUp]
//    public void SetUp()
//    {
//        logger = new Mock<ILogger<ChildService>>();
//        localizer = new Mock<IStringLocalizer<SharedResource>>();
//        mockRepository = new Mock<IEntityRepository<Child>>();
//    }

//    [Test]
//    public void ChildService_GetAll_ReturnsChildrenModels()
//    {
//        var expected = GetTestChildDTO().ToList();
//        mockRepository.Setup(m => m.GetAll()).Returns(Task.FromResult(GetTestChildEntities()));
//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        var actual = childService.GetAll().Result.ToList();

//        for (int i = 0; i < actual.Count; i++)
//        {
//            Assert.AreEqual(expected[i].Id, actual[i].Id);
//            Assert.AreEqual(expected[i].FirstName, actual[i].FirstName);
//            Assert.AreEqual(expected[i].LastName, actual[i].LastName);
//            Assert.AreEqual(expected[i].MiddleName, actual[i].MiddleName);
//        }
//    }

//    [Test]
//    public void ChildService__Create_AddsModel()
//    {
//        mockRepository.Setup(m => m.Create(It.IsAny<Child>())).Returns(Task.FromResult(createdChild));
//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        childService.Create(createdChildDTO);

//        mockRepository.Verify(x => x.Create(It.Is<Child>(c => c.Id == createdChild.Id && c.FirstName == createdChild.FirstName && c.LastName == createdChild.LastName && c.MiddleName == createdChild.MiddleName)), Times.Once);
//    }

//    [Test]
//    public void ChildService_Create_EntityIsNull_ThrowsArgumentNullException()
//    {
//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        ChildDto child = null;

//        Assert.ThrowsAsync<ArgumentNullException>(() => childService.Create(child));
//    }

//    [Test]
//    public void ChildService_Create_EntityIsInvalid_ThrowsArgumentException()
//    {
//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        ChildDto child = new ChildDto { Id = 20, FirstName = "fn3", LastName = "ln3", MiddleName = "mn3", DateOfBirth = new DateTime(DateTime.Now.Year + 1, 3, 20), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 };

//        Assert.ThrowsAsync<ArgumentException>(() => childService.Create(child));
//    }

//    [Test]
//    public void ChildService_GetById_ReturnsChildEntity()
//    {
//        var expected = GetTestChildDTO().ToList().First();
//        mockRepository.Setup(m => m.GetById(It.IsAny<long>())).Returns(Task.FromResult(GetTestChildEntities().First()));
//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        var actual = childService.GetByIdForCurrentUser(1).Result;

//        Assert.AreEqual(expected.Id, actual.Id);
//        Assert.AreEqual(expected.FirstName, actual.FirstName);
//        Assert.AreEqual(expected.LastName, actual.LastName);
//        Assert.AreEqual(expected.MiddleName, actual.MiddleName);
//    }

//    [Test]
//    public void ChildService_Update_ReturnsUpdatedChild()
//    {
//        ChildDto child = new ChildDto { Id = 1, FirstName = "fn11", LastName = "ln1", MiddleName = "mn11", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 };
//        Child childUpdated = new Child { Id = 1, FirstName = "fn11", LastName = "ln1", MiddleName = "mn11", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 };
//        mockRepository.Setup(m => m.Update(It.IsAny<Child>())).Returns(Task.FromResult(childUpdated));
//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        var actual = childService.UpdateChildForParetWithSpecifiedUserId(child).Result;

//        mockRepository.Verify(x => x.Update(It.Is<Child>(c => c.Id == childUpdated.Id && c.FirstName == childUpdated.FirstName && c.LastName == childUpdated.LastName && c.MiddleName == childUpdated.MiddleName)), Times.Once);
//    }

//    [Test]
//    public void ChildService_Update_EntityIsNull_ThrowsArgumentNullException()
//    {
//        ChildDto child = null;
//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        Assert.ThrowsAsync<ArgumentNullException>(() => childService.UpdateChildForParetWithSpecifiedUserId(child));
//    }

//    [Test]
//    public void ChildService_Update_ThrowsArgumentExceptionWrongDateOfBirth()
//    {
//        ChildDto child = new ChildDto { Id = 1, FirstName = "fn11", LastName = "ln1", MiddleName = "mn11", DateOfBirth = new DateTime(2023, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 };
//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        Assert.ThrowsAsync<ArgumentException>(() => childService.UpdateChildForParetWithSpecifiedUserId(child));
//    }

//    [Test]
//    public void ChildService_Update_ThrowsArgumentExceptionEmptyFirstname()
//    {
//        ChildDto child = new ChildDto { Id = 1, FirstName = string.Empty, LastName = "ln1", MiddleName = "mn11", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 };

//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        Assert.ThrowsAsync<ArgumentException>(() => childService.UpdateChildForParetWithSpecifiedUserId(child));
//    }

//    [Test]
//    public void ChildService_Update_ThrowsArgumentExceptionEmptyLastname()
//    {
//        ChildDto child = new ChildDto { Id = 1, FirstName = "fn11", LastName = string.Empty, MiddleName = "mn11", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 };

//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        Assert.ThrowsAsync<ArgumentException>(() => childService.UpdateChildForParetWithSpecifiedUserId(child));
//    }

//    [Test]
//    public void ChildService_Update_ThrowsArgumentExceptionEmptyPatronymic()
//    {
//        ChildDto child = new ChildDto { Id = 1, FirstName = "fn11", LastName = "ln1", MiddleName = string.Empty, DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 };

//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        Assert.ThrowsAsync<ArgumentException>(() => childService.UpdateChildForParetWithSpecifiedUserId(child));
//    }

//    [Test]
//    public void ChildService_Delete_DeletesChild()
//    {
//        ChildDto child = new ChildDto { Id = 1, FirstName = "fn1", LastName = "ln1", MiddleName = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 };
//        mockRepository.Setup(m => m.Delete(It.IsAny<Child>()));
//        mockRepository.Setup(m => m.GetById(It.IsAny<long>())).Returns(Task.FromResult(GetTestChildEntities().First()));

//        IChildService childService = new ChildService(mockRepository.Object, logger.Object, localizer.Object);

//        childService.DeleteChildCheckingItsUserIdProperty(1);

//        mockRepository.Verify(x => x.Delete(It.Is<Child>(c => c.Id == child.Id)), Times.Once);
//    }

//    private IEnumerable<ChildDto> GetTestChildDTO()
//    {
//        return new List<ChildDto>()
//        {
//            new ChildDto { Id = 1, FirstName = "fn1", LastName = "ln1", MiddleName = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 },
//            new ChildDto { Id = 2, FirstName = "fn2", LastName = "ln2", MiddleName = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = 2, SocialGroupId = 1 },
//            new ChildDto { Id = 3, FirstName = "fn3", LastName = "ln3", MiddleName = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 },
//        };
//    }

//    private IEnumerable<Child> GetTestChildEntities()
//    {
//        return new List<Child>()
//        {
//            new Child { Id = 1, FirstName = "fn1", LastName = "ln1", MiddleName = "mn1", DateOfBirth = new DateTime(2003, 11, 9), Gender = Gender.Male, ParentId = 1, SocialGroupId = 2 },
//            new Child { Id = 2, FirstName = "fn2", LastName = "ln2", MiddleName = "mn2", DateOfBirth = new DateTime(2004, 11, 8), Gender = Gender.Female, ParentId = 2, SocialGroupId = 1 },
//            new Child { Id = 3, FirstName = "fn3", LastName = "ln3", MiddleName = "mn3", DateOfBirth = new DateTime(2006, 11, 2), Gender = Gender.Male, ParentId = 1, SocialGroupId = 1 },
//        };
//    }
//}
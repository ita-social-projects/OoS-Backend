using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class ChildServiceTests
{
    private Mock<IParentRepository> parentRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<Guid, Child>> childRepositoryMock;
    private Mock<IEntityRepositorySoftDeleted<long, SocialGroup>> socialGroupRepositoryMock;
    private Mock<ILogger<ChildService>> loggerMock;
    private Mock<IMapper> mapperMock;
    private Mock<IApplicationRepository> applicationRepositoryMock;
    private Mock<IOptions<ParentConfig>> parentConfigMock;
    private ChildService childService;

    private OffsetFilter offsetFilter = new OffsetFilter();

    [SetUp]
    public void SetUp()
    {
        parentRepositoryMock = new Mock<IParentRepository>();
        childRepositoryMock = new Mock<IEntityRepositorySoftDeleted<Guid, Child>>();
        socialGroupRepositoryMock = new Mock<IEntityRepositorySoftDeleted<long, SocialGroup>>();
        loggerMock = new Mock<ILogger<ChildService>>();
        mapperMock = new Mock<IMapper>();
        applicationRepositoryMock = new Mock<IApplicationRepository>();
        parentConfigMock = new Mock<IOptions<ParentConfig>>();

        childService = new ChildService(
            childRepositoryMock.Object,
            parentRepositoryMock.Object,
            socialGroupRepositoryMock.Object,
            loggerMock.Object,
            mapperMock.Object,
            applicationRepositoryMock.Object,
            parentConfigMock.Object);
    }

    [Test]
    public async Task GetApprovedByWorkshopId_ExistingChildren_ShouldReturnChildren()
    {
        var workshopId = Guid.NewGuid();
        var children = new List<Child>()
        {
            new Child() { Id = Guid.NewGuid() },
            new Child() { Id = Guid.NewGuid() },
            new Child() { Id = Guid.NewGuid() },
        };
        var applications = new List<Application>()
        {
            new Application() { WorkshopId = workshopId, ChildId = children[0].Id, Status = ApplicationStatus.Approved },
            new Application() { WorkshopId = workshopId, ChildId = children[1].Id, Status = ApplicationStatus.StudyingForYears },
            new Application() { WorkshopId = workshopId, ChildId = children[2].Id, Status = ApplicationStatus.Pending },
        };
        var expectedTotalAmount = 2;

        applicationRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Application, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync((Expression<Func<Application, bool>> filter, string _) =>
            {
                var predicate = filter.Compile();
                return applications.Where(predicate);
            });
        childRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Child, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Child, object>>, SortDirection>>(),
                false))
            .Returns(new List<Child>().AsTestAsyncEnumerableQuery());
        mapperMock.Setup(mapper => mapper.Map<List<ChildDto>>(It.IsAny<List<Child>>()))
            .Returns(new List<ChildDto>());

        var result = await childService.GetApprovedByWorkshopId(workshopId, offsetFilter);

        Assert.IsNotNull(result);
        Assert.AreEqual(expectedTotalAmount, result.TotalAmount);
    }

    [Test]
    public async Task GetApprovedByWorkshopId_NoExistingChildren_ShouldReturnEmptySearchResult()
    {
        var workshopId = Guid.NewGuid();
        var children = new List<Child>()
        {
            new Child() { Id = Guid.NewGuid() },
            new Child() { Id = Guid.NewGuid() },
            new Child() { Id = Guid.NewGuid() },
        };
        var applications = new List<Application>()
        {
            new Application() { WorkshopId = workshopId, ChildId = children[0].Id, Status = ApplicationStatus.Left },
            new Application() { WorkshopId = workshopId, ChildId = children[1].Id, Status = ApplicationStatus.Completed },
            new Application() { WorkshopId = workshopId, ChildId = children[2].Id, Status = ApplicationStatus.Pending },
        };

        applicationRepositoryMock.Setup(x => x.GetByFilter(It.IsAny<Expression<Func<Application, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync((Expression<Func<Application, bool>> filter, string _) =>
            {
                var predicate = filter.Compile();
                return applications.Where(predicate);
            });
        childRepositoryMock.Setup(r => r.Get(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Expression<Func<Child, bool>>>(),
                It.IsAny<Dictionary<Expression<Func<Child, object>>, SortDirection>>(),
                false))
            .Returns(new List<Child>().AsTestAsyncEnumerableQuery());
        mapperMock.Setup(mapper => mapper.Map<List<ChildDto>>(It.IsAny<List<Child>>()))
            .Returns(new List<ChildDto>());

        var result = await childService.GetApprovedByWorkshopId(workshopId, offsetFilter);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalAmount);
    }

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

    [Test]
    public async Task ChildService_DeleteWithTechAdminRole_DeletesChild()
    {
        // Arrange
        var child = ChildGenerator.Generate().WithGeneratedParent();

        var childList = new List<Child> { child }.BuildMock();

        childRepositoryMock.Setup(m => m.GetByFilterNoTracking(It.IsAny<Expression<Func<Child, bool>>>(), It.IsAny<string>()))
            .Returns(childList);

        childRepositoryMock.Setup(m => m.Delete(child)).Returns(Task.CompletedTask);
        applicationRepositoryMock.Setup(x => x.DeleteChildApplications(child.Id)).Returns(Task.CompletedTask);

        // Act
        await childService.DeleteChildCheckingItsUserIdProperty(child.Id, child.Parent.UserId, true);

        // Assert
        Mock.VerifyAll();
    }

    [Test]
    public async Task ChildService_DeleteWithParentRole_DeletesChild()
    {
        // Arrange
        var child = ChildGenerator.Generate().WithGeneratedParent();

        var childList = new List<Child> { child }.BuildMock();

        childRepositoryMock.Setup(m => m.GetByFilterNoTracking(It.IsAny<Expression<Func<Child, bool>>>(), It.IsAny<string>()))
            .Returns(childList);

        childRepositoryMock.Setup(m => m.Delete(child)).Returns(Task.CompletedTask);
        applicationRepositoryMock.Setup(x => x.DeleteChildApplications(child.Id)).Returns(Task.CompletedTask);

        // Act
        await childService.DeleteChildCheckingItsUserIdProperty(child.Id, child.Parent.UserId, false);

        // Assert
        Mock.VerifyAll();
    }

    [Test]
    public void ChildService_DeleteWithoutTechAdminPermissionAndInvalidUserId_ShouldNotDeleteChild()
    {
        // Arrange
        var child = new Child()
        {
            Id = Guid.NewGuid(),
            Parent = new Parent() { UserId = Guid.NewGuid().ToString() },
        };

        var childList = new List<Child> { child }.BuildMock();

        childRepositoryMock.Setup(m => m.GetByFilterNoTracking(It.IsAny<Expression<Func<Child, bool>>>(), It.IsAny<string>()))
            .Returns(childList);

        childRepositoryMock.Setup(m => m.Delete(It.IsAny<Child>())).Returns(Task.CompletedTask);
        applicationRepositoryMock.Setup(x => x.DeleteChildApplications(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        // Act and assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(() => childService.DeleteChildCheckingItsUserIdProperty(child.Id, Guid.NewGuid().ToString(), false));
    }
}

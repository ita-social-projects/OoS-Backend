using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Config.ExternalUriModels;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace OutOfSchool.AuthServer.Tests.Services;

public class EmployeeServiceTests
{
    private Mock<IMapper> fakeMapper;
    private EmployeeRepository employeeRepository;
    private OutOfSchoolDbContext context;
    private Mock<UserManager<User>> fakeUserManager;
    private Mock<IOptions<GrpcConfig>> fakeGrpcConfig;
    private Mock<IOptions<ChangesLogConfig>> fakeChangesLogConfig;
    private Mock<IOptions<AngularClientScopeExternalUrisConfig>> fakeExternalUrisConfig;
    private Mock<IOptions<HostsConfig>> fakeHostsConfig;
    private Mock<IUrlHelper> fakeUrlHelper;

    private IEmployeeService employeeService;

    [SetUp]
    public void SetUp()
    {
        fakeMapper = new Mock<IMapper>();
        fakeUserManager = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<User>>().Object,
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<User>>>().Object);

        context = GetContext();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        employeeRepository = new EmployeeRepository(context);

        fakeGrpcConfig = new Mock<IOptions<GrpcConfig>>();
        fakeGrpcConfig.Setup(x => x.Value).Returns(new GrpcConfig());

        fakeExternalUrisConfig = new Mock<IOptions<AngularClientScopeExternalUrisConfig>>();
        fakeExternalUrisConfig.Setup(x => x.Value).Returns(new AngularClientScopeExternalUrisConfig());

        fakeChangesLogConfig = new Mock<IOptions<ChangesLogConfig>>();
        var changesLogConfig = new ChangesLogConfig
        {
            TrackedProperties = new Dictionary<string, string[]>
            {
                { "ProviderAdmin", Array.Empty<string>() }
            }
        };
        fakeChangesLogConfig.Setup(x => x.Value).Returns(changesLogConfig);

        fakeHostsConfig = new Mock<IOptions<HostsConfig>>();
        var hostsConfig = new HostsConfig()
        {
            BackendUrl = "http://localhost:5443"
        };
        fakeHostsConfig.Setup(x => x.Value).Returns(hostsConfig);

        fakeUrlHelper = new Mock<IUrlHelper>();

        employeeService = new EmployeeService(
            fakeMapper.Object,
            employeeRepository,
            new Mock<ILogger<EmployeeService>>().Object,
            new Mock<IEmailSenderService>().Object,
            fakeUserManager.Object,
            context,
            new Mock<IRazorViewToStringRenderer>().Object,
            new Mock<IEmployeeChangesLogService>().Object,
            fakeGrpcConfig.Object,
            fakeExternalUrisConfig.Object,
            fakeChangesLogConfig.Object,
            fakeHostsConfig.Object);
    }

    [Test]
    public async Task CreateProviderAdminAsync_WhenParametersIsValid_ReturnsOkResponse()
    {
        // Arrange
        var workshops = WorkshopGenerator.Generate(3);
        var user = UserGenerator.Generate();

        var createEmployeeDto = AdminGenerator.GenerateCreateEmployeeDto();
        createEmployeeDto.ManagedWorkshopIds = [workshops[1].Id ];
        
        var employee = EmployeesGenerator.Generate();
        employee.ManagedWorkshops = workshops;
        
        IUrlHelper url = fakeUrlHelper.Object;
        var userId = string.Empty;
        var userRole = "employee";

        context.AddRange(workshops);
        await context.SaveChangesAsync();

        fakeMapper.Setup(x => x.Map<User>(createEmployeeDto)).Returns(user);
        fakeUserManager.Setup(x => x.CreateAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        fakeUserManager.Setup(x => x.AddToRoleAsync(user, userRole))
            .ReturnsAsync(IdentityResult.Success);
        fakeMapper.Setup(x => x.Map<Employee>(createEmployeeDto))
            .Returns(employee);

        // Act
        var result = await employeeService.CreateEmployeeAsync(createEmployeeDto, url, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.Created, result.HttpStatusCode);
        createEmployeeDto.Should().BeEquivalentTo(result.Result);
    }

    [Test]
    public async Task CreateProviderAdminAsync_WhenEmailsAreIdentical_ReturnsBadRequest()
    {
        // Arrange
        var user = UserGenerator.Generate();
        var createProviderAdminDto = AdminGenerator.GenerateCreateEmployeeDto();
        user.Email = createProviderAdminDto.Email;

        IUrlHelper url = fakeUrlHelper.Object;
        var userId = string.Empty;

        context.Add(user);
        await context.SaveChangesAsync();

        fakeMapper.Setup(x => x.Map<User>(createProviderAdminDto)).Returns(user);


        // Act
        var result = await employeeService.CreateEmployeeAsync(createProviderAdminDto, url, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.HttpStatusCode);
    }

    [Test]
    public void HostsConfig_WhenValueNull_ThrowsException()
    {
        // Arrange
        var mockHostsConfig = new Mock<IOptions<HostsConfig>>();
        mockHostsConfig.Setup(x => x.Value).Returns((HostsConfig)null);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmployeeService(
            fakeMapper.Object,
            employeeRepository,
            new Mock<ILogger<EmployeeService>>().Object,
            new Mock<IEmailSenderService>().Object,
            fakeUserManager.Object,
            context,
            new Mock<IRazorViewToStringRenderer>().Object,
            new Mock<IEmployeeChangesLogService>().Object,
            fakeGrpcConfig.Object,
            fakeExternalUrisConfig.Object,
            fakeChangesLogConfig.Object,
            mockHostsConfig.Object));
    }

    private static TestOutOfSchoolDbContext GetContext()
    {
        return new TestOutOfSchoolDbContext(
            new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);
    }
}
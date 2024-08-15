using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Moq;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.WebApi.Tests.Services.ProviderServicesTests;

public static class ProviderTestsHelper
{
    public static Mock<IProviderRepository> CreateProvidersRepositoryMock(IEnumerable<Provider> providersCollection)
    {
        var providersRepository = new Mock<IProviderRepository>();

        providersRepository.Setup(r => r.ExistsUserId(It.IsAny<string>()))
            .Returns<string>((userId) => providersCollection.Any(p => p.UserId.Equals(userId)));

        return providersRepository;
    }

    public static Mock<IEntityRepositorySoftDeleted<string, User>> CreateUsersRepositoryMock(User fakeUser)
    {
        var usersRepository = new Mock<IEntityRepositorySoftDeleted<string, User>>();
        usersRepository.Setup(r => r.GetAll()).Returns(Task.FromResult<IEnumerable<User>>(new List<User> { fakeUser }));
        usersRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), string.Empty)).Returns(Task.FromResult<IEnumerable<User>>(new List<User> { fakeUser }));

        return usersRepository;
    }
}

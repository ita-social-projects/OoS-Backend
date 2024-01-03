using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class ErrorResponseTest
{
    private readonly HttpStatusCode httpBadRequest = HttpStatusCode.BadRequest;
    private ApiErrorResponse emailAlreadyTaken;

    [SetUp]
    public void SetUp()
    {
        emailAlreadyTaken = new ApiErrorResponse(new List<ApiError>()
            {
                    ApiErrorsTypes.Common.EmailAlreadyTaken("ProviderAdmin","email@gmail.com"),
            });
    }

    [Test]
    public void CreateNewErrorResponse_BadRequest_ReturnsNewErrorResponse()
    {
        // Act
        var result = ErrorResponse.BadRequest(emailAlreadyTaken);

        // Assert
        Assert.AreEqual(httpBadRequest, result.HttpStatusCode);
        Assert.AreEqual(1, result.ApiErrorResponse.ApiErrors.Count);
    }
}

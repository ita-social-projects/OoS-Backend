using System.Collections.Generic;
using NUnit.Framework;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class ApiErrorResponseTest
{
    private readonly string group = "newGroup";
    private readonly string code = "newCode";
    private readonly string message = "newMessage";

    private ApiError apiError;
    private List<ApiError> apiErrors;
    private ApiErrorResponse apiErrorResponse;

    [SetUp]
    public void SetUp()
    {
        apiError = new ApiError(group, code, message);
        apiErrors = new List<ApiError>
        {
            apiError,
        };
        apiErrorResponse = new ApiErrorResponse();
    }

    [Test]
    public void CreateNewApiErrorResponse_WithApiErrors_ReturnsNewApiErrorResponse()
    {
        // act
        var apiErrorResponse = new ApiErrorResponse(apiErrors);

        // assert
        Assert.AreEqual(apiErrorResponse.ApiErrors, apiErrors);
    }

    [Test]
    public void AddNewApiError_ToApiErrorResponse_AdedNewApiError()
    {
        // act
        apiErrorResponse.AddApiError(apiError);

        // assert
        Assert.AreEqual(apiErrorResponse.ApiErrors, apiErrors);
    }
}

using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using OutOfSchool.Common.Responces;
using OutOfSchool.WebApi.Services;
using System;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class ApiErrorTest
{
    private readonly string group = "newGroup";
    private readonly string code = "newCode";
    private readonly string message = "newMessage";

    private readonly string nullGroupExceptionMessage = "Group must be non-empty value (Parameter 'group')";
    private readonly string nullCodeExceptionMessage = "Code must be non-empty value (Parameter 'code')";
    private readonly string nullMessageExceptionMessage = "Message must be non-empty value (Parameter 'message')";

    [Test]
    public void CreateNewApiError_GroupIsNull_ThrowsArgumentException()
    {
        // act & assert
        var exception = Assert.Throws<ArgumentException>(() => new ApiError(null, code, message));

        Assert.AreEqual(nullGroupExceptionMessage, exception.Message);
        Assert.AreEqual("group", exception.ParamName);
    }

    [Test]
    public void CreateNewApiError_CodeIsNull_ThrowsArgumentException()
    {
        // act & assert
        var exception = Assert.Throws<ArgumentException>(() => new ApiError(group, null, message));

        Assert.AreEqual(nullCodeExceptionMessage, exception.Message);
        Assert.AreEqual("code", exception.ParamName);
    }

    [Test]
    public void CreateNewApiError_MessageIsNull_ThrowsArgumentException()
    {
        // act & assert
        var exception = Assert.Throws<ArgumentException>(() => new ApiError(group, code, null));

        Assert.AreEqual(nullMessageExceptionMessage, exception.Message);
        Assert.AreEqual("message", exception.ParamName);
    }

    [Test]
    public void CreateNewApiError_WithAllParams_ReturnsNewApiError()
    {
        // act
        var apiError = new ApiError(group, code, message);

        // assert
        Assert.AreEqual(apiError.Group, group);
        Assert.AreEqual(apiError.Code, code);
        Assert.AreEqual(apiError.Message, message);
    }
}

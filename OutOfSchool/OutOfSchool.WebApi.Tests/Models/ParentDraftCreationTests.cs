using System;
using NUnit.Framework;
using OutOfSchool.Common;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Tests.Models;

[TestFixture]
public class ParentDraftCreationTests
{
    [Test]
    public void CreateDraft_ReturnsMaleGender()
    {
        // Assert
        Assert.AreEqual(Gender.Male, Parent.CreateDraft("test", DateTime.UtcNow).Gender);
    }

    [Test]
    public void CreateDraft_ReturnsMaleValidAge()
    {
        // Arrange
        var expectedDate = DateTime.UtcNow.AddYears(-Constants.AdultAge).Date;

        // Assert
        Assert.AreEqual(expectedDate, Parent.CreateDraft("test", DateTime.UtcNow).DateOfBirth.GetValueOrDefault().Date);
    }

    [Test]
    public void CreateDraft_ReturnsMaleValidUserId()
    {
        // Assert
        Assert.AreEqual("test", Parent.CreateDraft("test", DateTime.UtcNow).UserId);
    }
}

using NUnit.Framework;
using OutOfSchool.BusinessLogic.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Tests.Validators;

[TestFixture]
public class MaxDecimalPlacesAttributeTests
{
    [Test]
    public void MaxDecimalPlaces_ValidInput_ReturnsSuccess()
    {
        var attribute = new MaxDecimalPlacesAttribute(2);
        var result = attribute.GetValidationResult(12.34m, new ValidationContext(new object()));
        Assert.AreEqual(ValidationResult.Success, result);
    }

    [Test]
    public void MaxDecimalPlaces_InvalidInput_ReturnsError()
    {
        var attribute = new MaxDecimalPlacesAttribute(2);
        var result = attribute.GetValidationResult(12.345m, new ValidationContext(new object()));
        Assert.IsNotNull(result);
        Assert.AreEqual("The field must not have more than 2 decimal places.", result.ErrorMessage);
    }
}

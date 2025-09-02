using OutOfSchool.SportsRegistryApiClient.Validators;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.SportRegistryApiClient.Tests.Validators;

[TestFixture]
public class PhoneListValidationAttributeTests
{
    private class TestModel
    {
        [PhoneListValidation]
        public List<string>? Phones { get; set; }
    }

    [Test]
    [TestCase("123")]
    [TestCase("0123456789", "123abc")]
    [TestCase("0123456789", "123abc")]
    public void InvalidPhones_ShouldFailValidation(params string[] phones)
    {
        var model = new TestModel { Phones = phones.ToList() };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(model, context, results, true);

        Assert.IsTrue(results.Any());
    }

    [Test]
    public void ValidPhones_ShouldPassValidation()
    {
        var model = new TestModel { Phones = new List<string> { "0123456789", "9876543210" } };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, context, results, true);

        Assert.IsTrue(isValid);
        Assert.IsEmpty(results);
    }
}

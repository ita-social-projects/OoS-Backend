using OutOfSchool.SportsRegistryApiClient.Models.Enums;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.SportRegistryApiClient.Tests.Validators;

[TestFixture]
public class SportsSectionPostRequestValidationTests
{
    private SportsSectionPostRequest model = null!;

    [SetUp]
    public void SetUp()
    {
        model = CreateValidModel();
    }

    [Test]
    public void ValidData_ShouldPassValidation()
    {
        var context = new ValidationContext(model);
        var results = model.Validate(context).ToList();
        Assert.IsEmpty(results);
    }

    [Test]
    public void NegativeAge_ShouldFailValidation()
    {
        model.SectionAgeFrom = -1;

        var context = new ValidationContext(model);
        var results = model.Validate(context).ToList();

        Assert.That(HasError(results, "Age values cannot be negative"));
    }

    [Test]
    public void AgeFromGreaterThanAgeTo_ShouldFailValidation()
    {
        model.SectionAgeFrom = 20;
        model.SectionAgeTo = 15;

        var context = new ValidationContext(model);
        var results = model.Validate(context).ToList();

        Assert.That(HasError(results, "SectionAgeFrom cannot be greater than SectionAgeTo"));
    }

    [Test]
    [TestCase("//invalid_url")]
    [TestCase("htp://example.com")]
    [TestCase("example.com")]
    [TestCase("http:/example.com")]
    [TestCase("www.example.com/photo")]
    public void InvalidPhotoUrl_ShouldFailValidation(string invalidUrl)
    {
        model.SectionPhotos = new List<string> { invalidUrl };

        var context = new ValidationContext(model);
        var results = model.Validate(context).ToList();

        Assert.That(HasError(results,"Invalid photo URLs"),
            $"Expected validation to fail for URL: {invalidUrl}");
    }

    [TestCase("not-an-email")]
    [TestCase("user@com")]
    [TestCase("user@.com")]
    [TestCase("user@@example.com")]
    public void InvalidEmail_ShouldFailValidation(string email)
    {
        model.SectionEmail = email;

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);

        Assert.That(HasError(results, "Invalid email format"));
    }

    [Test]
    [TestCase("https://www.facebook.com/page")]
    [TestCase("http://facebook.com/page")]
    [TestCase("https://facebook.com/page")]
    public void ValidFacebookUrl_ShouldPass(string url)
    {
        model.SectionFacebookUrl = url;
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        Assert.IsFalse(results.Any(r => r.MemberNames.Contains(nameof(model.SectionFacebookUrl))));
    }

    [Test]
    [TestCase("ftp://facebook.com/page")]
    [TestCase("htp://facebook.com/page")]
    [TestCase("http://facebookk.com/page")]
    [TestCase("//facebook.com/page")]
    [TestCase("https://example.com")]
    [TestCase("facebook.com")]
    public void InvalidFacebookUrl_ShouldFail(string url)
    {
        model.SectionFacebookUrl = url;
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(model.SectionFacebookUrl))));
    }

    [Test]
    [TestCase("https://www.instagram.com/page")]
    [TestCase("http://instagram.com/page")]
    [TestCase("https://instagram.com/page")]
    public void ValidInstagramUrl_ShouldPass(string url)
    {
        model.SectionInstagramUrl = url;
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        Assert.IsFalse(results.Any(r => r.MemberNames.Contains(nameof(model.SectionInstagramUrl))));
    }

    [Test]
    [TestCase("ftp://instagram.com/page")]
    [TestCase("https://example.com")]
    [TestCase("instagram.com")]
    public void InvalidInstagramUrl_ShouldFail(string url)
    {
        model.SectionInstagramUrl = url;
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(model.SectionInstagramUrl))));
    }

    [Test]
    public void PracticePeriodFromAfterTo_ShouldFailValidation()
    {
        model.SectionPracticePeriodDateFrom = "30:09";
        model.SectionPracticePeriodDateTo = "01:09";

        var context = new ValidationContext(model);
        var results = model.Validate(context).ToList();

        Assert.That(HasError(results, "Practice period start date cannot be after end date"));
    }

    [Test]
    public void EmptySchedule_ShouldFailValidation()
    {
        model.SectionSchedule.Clear();

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);

        Assert.That(HasError(results, "At least one section schedule is required"));
    }

    private SportsSectionPostRequest CreateValidModel()
    {
        return new SportsSectionPostRequest
        {
            OrganizationCode = "12345678",
            SectionName = "Test Section",
            SectionSportKindDictIdCode = 1,
            SectionAgeFrom = 10,
            SectionAgeTo = 15,
            SectionAddressLocalityDictIdCode = "001",
            SectionAddressStreet = "Main St",
            SectionAddressHouse = "1",
            SectionDescription = "Desc",
            SectionRegistrationFlow = "Flow",
            SectionPhones = new List<string> { "0123456789" },
            SectionEmail = "test@example.com",
            SectionRegistrationFormUrl = "https://forms.example.com",
            SectionPracticeFormat = SectionPracticeFormat.ONLINE,
            SectionPracticeCost = 100,
            SectionMaxStudentsAmount = 20,
            SectionPracticePeriodDateFrom = "01:09",
            SectionPracticePeriodDateTo = "30:09",
            SectionSchedule = new List<SectionScheduleRequest>
            {
                new SectionScheduleRequest
                {
                    SectionScheduleWeekday = Weekday.MONDAY,
                    SectionScheduleTimeFrom = "09:00:00",
                    SectionScheduleTimeTo = "10:00:00"
                }
            },
            SectionPozashkillyaModerationStatus = ModerationStatus.ACTIVE,
            SectionPhotos = new List<string> { "https://example.com/photo.jpg" }
        };
    }

    private static bool HasError(List<ValidationResult> results, string expectedMessage) =>
    results.Any(r => r.ErrorMessage?.Contains(expectedMessage) == true);

}
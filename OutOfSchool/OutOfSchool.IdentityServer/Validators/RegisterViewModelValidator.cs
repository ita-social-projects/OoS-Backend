using FluentValidation;
using Microsoft.Extensions.Localization;
using OutOfSchool.IdentityServer.ViewModels;

namespace OutOfSchool.IdentityServer.Validators;

public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterViewModelValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.DateOfBirth)
          .Must(BeLessThanToday).WithMessage(localizer["You must be at least 18 years old."]);
    }

    private bool BeLessThanToday(DateTime birthDate) => birthDate < DateTime.Now;
}
using FluentValidation;
using Microsoft.Extensions.Localization;
using OutOfSchool.AuthCommon.ViewModels;

namespace OutOfSchool.AuthCommon.Validators;

public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
{
    public RegisterViewModelValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.DateOfBirth).Must(x => x != DateTime.MinValue)
            .WithMessage(localizer["Check the entered data. This field DateOfBirth is required."]);

        RuleFor(x => x.DateOfBirth)
          .Must(BeAdult).When(x => !x.ProviderRegistration).WithMessage(localizer["You must be at least 18 years old."]);
    }

    private bool BeAdult(DateTime birthDate) => birthDate < DateTime.Now.AddYears(-18);
}
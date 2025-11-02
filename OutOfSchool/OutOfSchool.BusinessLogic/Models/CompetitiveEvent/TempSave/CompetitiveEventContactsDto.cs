using System.ComponentModel.DataAnnotations;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.CustomValidation;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.TempSave;

public class CompetitiveEventContactsDto : CompetitiveEventDescriptionDto
{
    [CollectionNotEmpty(ErrorMessage = "At least one contact is required")]
    public List<ContactsDto> Contacts { get; set; } = [];

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from CompetitiveEventDescriptionDto
        foreach (var error in base.Validate(validationContext))
            yield return error;
    }
}
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.TempSave;

public class CompetitiveEventContactsDto : CompetitiveEventDescriptionDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [CollectionNotEmpty(ErrorMessage = "At least one contact is required")]
    public List<ContactsDto> Contacts { get; set; } = [];

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from CompetitiveEventDescriptionDto
        foreach (var error in base.Validate(validationContext))
            yield return error;
    }
}
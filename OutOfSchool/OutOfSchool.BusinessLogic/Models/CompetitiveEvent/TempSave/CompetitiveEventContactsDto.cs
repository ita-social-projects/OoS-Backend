using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEvent.TempSave;

public class CompetitiveEventContactsDto : CompetitiveEventDescriptionDto
{
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<ContactsDto> Contacts { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Run validations from CompetitiveEventBaseDto
        foreach (var error in base.Validate(validationContext))
            yield return error;
    }
}
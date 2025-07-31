using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.Services.Models.ContactInfo;
using System.ComponentModel.DataAnnotations;
using CompetitiveEventDraftModel = OutOfSchool.Services.Models.CompetitiveEventDrafts.CompetitiveEventDraft;

namespace OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
public class ModeratorCompetitiveEventDraftEditDto
{
    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; }

    [Required(ErrorMessage = "ShortTitle is required")]    
    public string ShortTitle { get; set; }

    public string DescriptionOfTheEnrollmentProcedure { get; set; }

    public IEnumerable<ContactsDto> Contacts { get; set; } = [];
}

public static class ModeratorCompetitiveEventDraftEditDtoExtensions
{
    public static CompetitiveEventDraftModel ToDraft(this ModeratorCompetitiveEventDraftEditDto dto, CompetitiveEventDraftModel model)
    { 
        model.CompetitiveEventDraftContent.Title = dto.Title;
        model.CompetitiveEventDraftContent.ShortTitle = dto.ShortTitle;
        model.CompetitiveEventDraftContent.DescriptionOfTheEnrollmentProcedure = dto.DescriptionOfTheEnrollmentProcedure;

        UpdateContactsForModeration(model.CompetitiveEventDraftContent.Contacts, dto.Contacts);

        return model;
    }

    private static void UpdateContactsForModeration(
        List<Contacts> existingContacts,
        IEnumerable<ContactsDto> moderationContacts)
    {
        if (existingContacts == null || moderationContacts == null)
            return;

        foreach (var moderationContact in moderationContacts)
        {
            // Find existing contact by Title + Address combination
            var existingContact = existingContacts.FirstOrDefault(c =>
                c.Title == moderationContact.Title &&
                c.Address != null &&
                moderationContact.Address != null &&
                c.Address.Equals(moderationContact.Address.ToModel()));

            if (existingContact != null)
            {
                existingContact.Phones = moderationContact.Phones?.ToModel() ?? [];
                existingContact.Emails = moderationContact.Emails?.ToModel() ?? [];
                existingContact.SocialNetworks = moderationContact.SocialNetworks?.ToModel() ?? [];
            }
        }
    }
}

using OutOfSchool.Services.Models.ContactInfo;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Extensions;

public static class WorkshopDraftContentExtensions
{
    /// <summary>
    /// Creates a deep copy of the moderator-editable fields of a <see cref="WorkshopDraftContent"/> object.
    /// Only fields that moderators are allowed to edit are included in the copy.
    /// </summary>
    /// <param name="source">The source <see cref="WorkshopDraftContent"/> to copy from.</param>
    /// <returns>
    /// A new <see cref="WorkshopDraftContent"/> instance containing a copy of the editable fields,
    /// or <c>null</c> if the source is <c>null</c>.
    /// </returns>
    public static WorkshopDraftContent DeepCopyModeratorEditable(this WorkshopDraftContent source)
    {
        if (source == null)
        {
            return null;
        }

        return new WorkshopDraftContent
        {
            Title = source.Title,
            ShortTitle = source.ShortTitle,
            CompetitiveSelectionDescription = source.CompetitiveSelectionDescription,
            PreferentialTermsOfParticipation = source.PreferentialTermsOfParticipation,
            EnrollmentProcedureDescription = source.EnrollmentProcedureDescription,
            InstitutionHierarchyId = source.InstitutionHierarchyId,
            WorkshopDescriptionItems = source.WorkshopDescriptionItems?
                .Select(x => new WorkshopDescriptionItemDraft
                {
                    SectionName = x.SectionName,
                    Description = x.Description
                })
                .ToList(),

            Contacts = source.Contacts?
                .Select(contact => new Contacts
                {
                    Title = contact.Title,
                    IsDefault = contact.IsDefault,
                    Address = contact.Address != null ? new ContactsAddress
                    {
                        Street = contact.Address.Street,
                        BuildingNumber = contact.Address.BuildingNumber,
                        Latitude = contact.Address.Latitude,
                        Longitude = contact.Address.Longitude,
                        GeoHash = contact.Address.GeoHash,
                        CATOTTGId = contact.Address.CATOTTGId
                    } : null,

                    Phones = contact.Phones?
                        .Select(phone => new PhoneNumber
                        {
                            Type = phone.Type,
                            Number = phone.Number
                        })
                        .ToList() ?? [],

                    Emails = contact.Emails?
                        .Select(email => new Email
                        {
                            Type = email.Type,
                            Address = email.Address
                        })
                        .ToList() ?? [],

                    SocialNetworks = contact.SocialNetworks?
                        .Select(sn => new SocialNetwork
                        {
                            Type = sn.Type,
                            Url = sn.Url
                        })
                        .ToList() ?? []
                })
                .ToList()
        };
    }
}

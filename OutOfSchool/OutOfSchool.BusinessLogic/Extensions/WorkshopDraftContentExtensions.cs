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
                .ToList()
        };
    }
}

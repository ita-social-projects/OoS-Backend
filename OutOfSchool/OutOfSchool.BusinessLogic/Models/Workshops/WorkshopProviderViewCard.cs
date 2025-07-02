using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopProviderViewCard : WorkshopBaseCard
{
    public uint AvailableSeats { get; set; } = uint.MaxValue;

    public int AmountOfPendingApplications { get; set; }

    public WorkshopStatus Status { get; set; }
    
    public int UnreadMessages { get; set; }
}

public static class WorkshopProviderViewCardExtensions
{
    public static WorkshopProviderViewCard ToProviderViewCard(this Workshop model)
    {
        var defaultContact = model.Contacts?.FirstOrDefault(c => c.IsDefault);

        return new()
        {
            Id = model.Id,
            ProviderTitle = model.ProviderTitle,
            ProviderTitleEn = model.ProviderTitleEn,
            ProviderOwnership = model.ProviderOwnership,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            PayRate = model.PayRate,
            FormOfLearning = model.FormOfLearning,
            CoverImageId = model.CoverImageId,
            MinAge = model.MinAge,
            MaxAge = model.MaxAge,
            CompetitiveSelection = model.CompetitiveSelection,
            Price = model.Price,
            DirectionIds = model.InstitutionHierarchy?.SubDirections?.Where(x => !x.IsDeleted).Select(d => d.DirectionId).ToList() ?? [],
            SubDirectionIds = model.InstitutionHierarchy?.SubDirections?.Where(sd => !sd.IsDeleted).Select(sd => sd.Id).ToList() ?? [],
            ProviderId = model.ProviderId,
            Address = defaultContact?.Address?.ToDto(),
            ProviderLicenseStatus = model.Provider?.LicenseStatus ?? default,
            LanguageOfEducationId = model.LanguageOfEducationId,
            LanguageOfEducationName = model.LanguageOfEducation?.Name,
            AvailableSeats = model.AvailableSeats,
            Status = model.Status,
        };
    }

    public static List<WorkshopProviderViewCard> ToProviderViewCard(this IEnumerable<Workshop> list)
        => list.MapToList(ToProviderViewCard);
}
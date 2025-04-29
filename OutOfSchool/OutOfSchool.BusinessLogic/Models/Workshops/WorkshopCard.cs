namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopCard : WorkshopBaseCard
{
    public Guid? InstitutionHierarchyId { get; set; }

    public Guid? InstitutionId { get; set; }

    public string Institution { get; set; }

    public uint AvailableSeats { get; set; } = uint.MaxValue;
}

public static class WorkshopCardExtensions
{
    public static WorkshopCard ToCard(this WorkshopES workshop)
        => new()
        {
            Id = workshop.Id,
            Title = workshop.Title,
            ShortTitle = workshop.ShortTitle,
            CoverImageId = workshop.CoverImageId,
            Rating = workshop.Rating,
            NumberOfRatings = workshop.NumberOfRatings,
            ProviderId = workshop.ProviderId,
            ProviderTitle = workshop.ProviderTitle,
            ProviderTitleEn = workshop.ProviderTitleEn,
            ProviderOwnership = workshop.ProviderOwnership,
            MinAge = workshop.MinAge,
            MaxAge = workshop.MaxAge,
            CompetitiveSelection = workshop.CompetitiveSelection,
            Price = workshop.Price,
            PayRate = workshop.PayRate,
            Address = workshop.Address?.ToDto(),
            InstitutionHierarchyId = workshop.InstitutionHierarchyId,
            InstitutionId = workshop.InstitutionId,
            Institution = workshop.Institution,
            DirectionIds = workshop.DirectionIds,
            AvailableSeats = workshop.AvailableSeats,
            TakenSeats = workshop.TakenSeats,
            ProviderLicenseStatus = workshop.ProviderLicenseStatus,
            FormOfLearning = workshop.FormOfLearning,            
        };

    public static List<WorkshopCard> ToCard(this IEnumerable<WorkshopES> list)
        => list.MapToList(ToCard);

    public static WorkshopCard ToCard(this Workshop workshop)
        => new()
        {
            Id = workshop.Id,
            ProviderTitle = workshop.ProviderTitle,
            ProviderTitleEn = workshop.ProviderTitleEn,
            ProviderOwnership = workshop.ProviderOwnership,
            ProviderId = workshop.ProviderId,
            ProviderLicenseStatus = workshop.Provider?.LicenseStatus ?? default,
            Title = workshop.Title,
            PayRate = workshop.PayRate,
            FormOfLearning = workshop.FormOfLearning,
            CoverImageId = workshop.CoverImageId,
            MinAge = workshop.MinAge,
            MaxAge = workshop.MaxAge,
            CompetitiveSelection = workshop.CompetitiveSelection,
            Price = workshop.Price,
            DirectionIds = workshop.InstitutionHierarchy?.SubDirections?.Where(x => !x.IsDeleted).Select(x => x.DirectionId).ToList() ?? [],
            Address = workshop.Contacts?.FirstOrDefault(c => c.IsDefault)?.Address?.ToDto(),
            InstitutionHierarchyId = workshop.InstitutionHierarchyId,
            InstitutionId = workshop.InstitutionHierarchy?.Institution?.Id,
            Institution = workshop.InstitutionHierarchy?.Institution?.Title,
            AvailableSeats = workshop.AvailableSeats,
        };

    public static List<WorkshopCard> ToCard(this IEnumerable<Workshop> list)
        => list.MapToList(ToCard);
}
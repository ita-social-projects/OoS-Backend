namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopFilterWithSettlements : WorkshopFilter
{
    public IEnumerable<long> SettlementsIds { get; set; } = new List<long>();
}

public static class WorkshopFilterWithSettlementsExtensions
{
    public static WorkshopFilterWithSettlements ToFilterWithSettlements(this WorkshopFilter filter)
        => new()
        {
            Ids = filter.Ids,
            SearchText = filter.SearchText,
            OrderByField = filter.OrderByField,
            MinAge = filter.MinAge,
            MaxAge = filter.MaxAge,
            IsFree = filter.IsFree,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            SubDirectionIds = filter.SubDirectionIds,
            MinStartTime = filter.MinStartTime,
            MaxStartTime = filter.MaxStartTime,
            Latitude = filter.Latitude,
            Longitude = filter.Longitude,
            Statuses = filter.Statuses,
            IsAppropriateAge = filter.IsAppropriateAge,
            IsAppropriateHours = filter.IsAppropriateHours,
            IsStrictWorkdays = filter.IsStrictWorkdays,
            CATOTTGId = filter.CATOTTGId,
            RadiusKm = filter.RadiusKm,
            InstitutionId = filter.InstitutionId,
            FormOfLearning = filter.FormOfLearning,
            AgeComposition = [],
            EducationalShift = [],
            IsSelfFinanced = false,
            IsPaid = filter.IsPaid,
            SpecialNeedsType = [],
            IsInclusive = false,
            AreThereBenefits = filter.AreThereBenefits,
            Coverage = filter.Coverage,
            PayRate = filter.PayRate,
            StudyPeriodStartDay = filter.StudyPeriodStartDay,
            StudyPeriodEndDay = filter.StudyPeriodEndDay,
            StudyPeriodStartMonth = filter.StudyPeriodStartMonth,
            StudyPeriodEndMonth = filter.StudyPeriodEndMonth,
        };
}

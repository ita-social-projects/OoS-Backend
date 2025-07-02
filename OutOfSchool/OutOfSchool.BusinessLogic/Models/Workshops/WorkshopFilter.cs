using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

/// <inheritdoc/>>
public class WorkshopFilter : OffsetFilter
{
    public List<Guid> Ids { get; set; } = new List<Guid>();

    public string SearchText { get; set; } = string.Empty;

    public string OrderByField { get; set; } = OrderBy.Rating.ToString();

    [Range(0, 120, ErrorMessage = "Field value should be in a range from 0 to 120")]
    public int MinAge { get; set; } = 0;

    [Range(0, 120, ErrorMessage = "Field value should be in a range from 0 to 120")]
    public int MaxAge { get; set; } = 100;

    public bool IsFree { get; set; } = false;

    [Range(0, int.MaxValue, ErrorMessage = "Field value should be in a range from 0 to 2 147 483 647")]
    public int MinPrice { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Field value should be in a range from 0 to 2 147 483 647")]
    public int MaxPrice { get; set; } = int.MaxValue;

    public List<long> SubDirectionIds { get; set; } = new List<long>();

    public string City { get; set; } = string.Empty;

    public List<DaysBitMask> Workdays { get; set; } = new List<DaysBitMask>();

    [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan MinStartTime { get; set; } = new TimeSpan(0, 0, 0);

    [Range(typeof(TimeSpan), "00:00:00", "23:59:59")]
    [JsonConverter(typeof(TimespanConverter))]
    public TimeSpan MaxStartTime { get; set; } = new TimeSpan(23, 59, 59);

    public decimal Latitude { get; set; } = 0;

    public decimal Longitude { get; set; } = 0;

    public IReadOnlyCollection<WorkshopStatus> Statuses { get; set; } = new List<WorkshopStatus>();

    public bool IsAppropriateAge { get; set; } = false;

    public bool IsAppropriateHours { get; set; } = false;

    public bool IsStrictWorkdays { get; set; } = false;

    public long LanguageOfEducationId { get; set; }

    public long CATOTTGId { get; set; } = default;

    [Range(2, 10, ErrorMessage = "Field value should be in a range from 2 to 10")]
    public int RadiusKm { get; set; } = 5;

    public Guid? InstitutionId { get; set; } = Guid.Empty;

    public IReadOnlyCollection<FormOfLearning> FormOfLearning { get; set; } = new List<FormOfLearning>();

    public IReadOnlyCollection<AgeComposition> AgeComposition { get; set; } = new List<AgeComposition>();

    public IReadOnlyCollection<EducationalShift> EducationalShift { get; set; } = new List<EducationalShift>();

    public bool IsSelfFinanced { get; set; }

    public bool IsPaid { get; set; }

    public IReadOnlyCollection<SpecialNeedsType> SpecialNeedsType { get; set; } = new List<SpecialNeedsType>();

    public bool IsInclusive { get; set; }

    public bool AreThereBenefits { get; set; }

    public IReadOnlyCollection<Coverage> Coverage { get; set; } = new List<Coverage>();

    public PayRateType PayRate { get; set; } = PayRateType.None;

    [Range(1, 31, ErrorMessage = "Day must be in range from 1 to 31")]
    public int? StudyPeriodStartDay { get; set; }

    [Range(1, 31, ErrorMessage = "Day must be in range from 1 to 31")]
    public int? StudyPeriodEndDay { get; set; }

    [Range(1, 12, ErrorMessage = "Month must be in range from 1 to 12")]
    public int? StudyPeriodStartMonth { get; set; }

    [Range(1, 12, ErrorMessage = "Month must be in range from 1 to 12")]
    public int? StudyPeriodEndMonth { get; set; }
}

public static class WorkshopFilterExtensions
{
    public static WorkshopFilterES ToES(this WorkshopFilter dto)
        => new()
        {    
            Ids = dto.Ids,
            SearchText = dto.SearchText,
            OrderByField = dto.OrderByField,
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            IsFree = dto.IsFree,
            MinPrice = dto.IsPaid && !dto.IsFree ? Math.Max(1, dto.MinPrice) : dto.MinPrice,
            MaxPrice = dto.IsFree && !dto.IsPaid ? 0 : dto.MaxPrice,
            SubDirectionIds = dto.SubDirectionIds,
            City = dto.City,
            Workdays = string.Join(' ', dto.Workdays ?? []),
            MinStartTime = dto.MinStartTime,
            MaxStartTime = dto.MaxStartTime,
            Size = dto.Size,
            From = dto.From,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Statuses = dto.Statuses,
            IsAppropriateAge = dto.IsAppropriateAge,
            IsAppropriateHours = dto.IsAppropriateHours,
            IsStrictWorkdays = dto.IsStrictWorkdays,
            CATOTTGId = dto.CATOTTGId,
            ElasticRadius = $"{dto.RadiusKm * 1000}m",
            InstitutionId = dto.InstitutionId,
            FormOfLearning = dto.FormOfLearning,
            AgeComposition = [],
            EducationalShift = [],
            IsSelfFinanced = false,
            IsPaid = dto.IsPaid,
            SpecialNeedsType = [],
            IsInclusive = false,
            AreThereBenefits = dto.AreThereBenefits,
            Coverage = dto.Coverage,
            PayRate = dto.PayRate,
            StudyPeriodStartDay = dto.StudyPeriodStartDay,
            StudyPeriodEndDay = dto.StudyPeriodEndDay,
            StudyPeriodStartMonth = dto.StudyPeriodStartMonth,
            StudyPeriodEndMonth = dto.StudyPeriodEndMonth,
            LanguageOfEducationId = dto.LanguageOfEducationId
        };
}

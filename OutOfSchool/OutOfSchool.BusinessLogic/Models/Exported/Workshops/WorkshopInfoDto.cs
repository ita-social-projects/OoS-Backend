using OutOfSchool.BusinessLogic.Models.Exported.Contacts;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Exported.Workshops;

public class WorkshopInfoDto : WorkshopInfoBaseDto, IExternalRatingInfo
{
    public Guid ProviderId { get; set; }

    public Guid? ParentWorkshopId { get; set; }

    public int TakenSeats { get; set; } = 0;

    public float Rating { get; set; }

    public int NumberOfRatings { get; set; }

    [EnumDataType(typeof(WorkshopStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

    [Required(ErrorMessage = "Workshop title is required")]
    [MinLength(1)]
    [MaxLength(60)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Workshop short title is required")]
    [MinLength(Constants.MinWorkshopShortTitleLength)]
    [MaxLength(Constants.MaxWorkshopShortTitleLength)]
    public string ShortTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Children's min age is required")]
    [Range(0, 120, ErrorMessage = "Min age should be a number from 0 to 120")]
    public int MinAge { get; set; }

    [Required(ErrorMessage = "Children's max age is required")]
    [Range(0, 120, ErrorMessage = "Max age should be a number from 0 to 120")]
    public int MaxAge { get; set; }

    [Required]
    public List<DateTimeRangeDto> DateTimeRanges { get; set; }

    public bool IsPaid { get; set; } = false;

    [Range(0, 100000, ErrorMessage = "Field value should be in a range from 1 to 100 000")]
    public decimal? Price { get; set; } = default;

    [EnumDataType(typeof(PayRateType), ErrorMessage = Constants.EnumErrorMessage)]
    public PayRateType? PayRate { get; set; } = PayRateType.Class;

    [Required(ErrorMessage = "Form of learning is required")]
    [EnumDataType(typeof(FormOfLearning), ErrorMessage = Constants.EnumErrorMessage)]
    public FormOfLearning FormOfLearning { get; set; }

    public uint AvailableSeats { get; set; } = uint.MaxValue;
    
    public bool IsSeatsLimit => AvailableSeats != uint.MaxValue;

    public bool CompetitiveSelection { get; set; }

    [MaxLength(Constants.MaxCompetitiveSelectionDescriptionLength)]
    public string CompetitiveSelectionDescription { get; set; }

    [CollectionNotEmpty(ErrorMessage = "At least one description is required")]
    public IEnumerable<WorkshopDescriptionItemInfo> WorkshopDescriptionItems { get; set; }

    public string Institution { get; set; }

    public string InstitutionHierarchy { get; set; }

    public TeacherInfoDto DefaultTeacher { get; set; }

    public List<long> DirectionIds { get; set; }
    
    public List<long> SubDirectionIds { get; set; }

    public IEnumerable<string> Keywords { get; set; } = default;

    public List<TeacherInfoDto> Teachers { get; set; }

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public bool IsSelfFinanced { get; set; } = false;

    public bool IsInclusive { get; set; } = false;
    
    [Required(ErrorMessage = "Workshop type is required")]
    [EnumDataType(typeof(WorkshopType), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopType WorkshopType { get; set; }
    
    [EnumDataType(typeof(SpecialNeedsType), ErrorMessage = Constants.EnumErrorMessage)]
    public SpecialNeedsType SpecialNeedsType { get; set; } = SpecialNeedsType.None;

    public string? LanguageOfEducation { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    public IList<string> ImageIds { get; set; }

    public List<string> Tags { get; set; }
    
    public Guid? DefaultTeacherId { get; set; }
    
    [MaxLength(Constants.EnrollmentProcedureDescription)]
    public string EnrollmentProcedureDescription { get; set; }

    public bool AreThereBenefits { get; set; } = default;

    [MaxLength(Constants.MaxPreferentialTermsOfParticipationLength)]
    public string PreferentialTermsOfParticipation { get; set; }

    [Required]
    [EnumDataType(typeof(EducationalShift), ErrorMessage = Constants.EnumErrorMessage)]
    public EducationalShift EducationalShift { get; set; } = EducationalShift.First;

    [Required(ErrorMessage = "Type of age composition is required")]
    [EnumDataType(typeof(AgeComposition), ErrorMessage = Constants.EnumErrorMessage)]
    public AgeComposition AgeComposition { get; set; } = AgeComposition.SameAge;

    [EnumDataType(typeof(Coverage), ErrorMessage = Constants.EnumErrorMessage)]
    public Coverage Coverage { get; set; } = Coverage.School;
    
    public List<ContactsInfoDto> Contacts { get; set; }
}

public static class WorkshopInfoDtoExtensions
{
    public static WorkshopInfoBaseDto ToBaseInfoDto(this Workshop model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.Status == WorkshopStatus.Archived,
        };

    public static WorkshopInfoDto ToInfoDto(this Workshop model)
        => new()
        {
            Id = model.Id,
            IsDeleted = model.Status == WorkshopStatus.Archived,
            ProviderId = model.ProviderId,
            ParentWorkshopId = model.ParentWorkshopId,
            Status = model.Status,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            MinAge = model.MinAge,
            MaxAge = model.MaxAge,
            DateTimeRanges = model.DateTimeRanges?.ToNotDeletedDto() ?? [],
            IsPaid = model.IsPaid,
            Price = model.Price,
            PayRate = model.PayRate,
            FormOfLearning = model.FormOfLearning,
            AvailableSeats = model.AvailableSeats,
            CompetitiveSelection = model.CompetitiveSelection,
            CompetitiveSelectionDescription = model.CompetitiveSelectionDescription,
            WorkshopDescriptionItems = model.WorkshopDescriptionItems?.ToNotDeletedInfo() ?? [],
            Institution = model.InstitutionHierarchy?.Institution?.Title,
            InstitutionHierarchy = model.InstitutionHierarchy?.Title,
            DefaultTeacher = model.DefaultTeacher?.ToInfoDto(),
            DirectionIds = model.InstitutionHierarchy?.SubDirections?.Where(x => !x.IsDeleted && !x.Direction.IsDeleted).Select(d => d.DirectionId).ToList(),
            SubDirectionIds = model.InstitutionHierarchy?.SubDirections?.Where(x => !x.IsDeleted).Select(x => x.Id).ToList(),
            Keywords = model.Keywords?.Split(Constants.MappingSeparator, StringSplitOptions.None) ?? [],
            Teachers = model.Teachers?.ToInfoDto(),
            ActiveFrom = model.ActiveFrom,
            ActiveTo = model.ActiveTo,
            IsSelfFinanced = model.IsSelfFinanced,
            IsInclusive = model.IsInclusive,
            WorkshopType = model.WorkshopType,
            SpecialNeedsType = model.SpecialNeedsType,
            CoverImageId = model.CoverImageId,
            ImageIds = model.Images?.Select(x => x.ExternalStorageId).ToList() ?? [],
            Tags = model.Tags?.Select(x => x.Name).ToList() ?? [],
            DefaultTeacherId = model.DefaultTeacherId,
            EnrollmentProcedureDescription = model.EnrollmentProcedureDescription,
            AreThereBenefits = model.AreThereBenefits,
            PreferentialTermsOfParticipation = model.PreferentialTermsOfParticipation,
            EducationalShift = model.EducationalShift,
            AgeComposition = model.AgeComposition,
            Coverage = model.Coverage,
            Contacts = model.Contacts?.ToInfoDto(),
            LanguageOfEducation = model.LanguageOfEducation?.Name,
        };

    public static List<WorkshopInfoBaseDto> ToBaseOrInfoDto(this IEnumerable<Workshop> list)
        => list.MapToList(x => x.Status == WorkshopStatus.Archived ? x.ToBaseInfoDto() : x.ToInfoDto());
}
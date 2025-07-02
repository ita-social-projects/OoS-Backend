using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using OutOfSchool.BusinessLogic.Models.Tag;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.Workshop;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Models.Workshops;

public class WorkshopDto : WorkshopCreateUpdateDto, IHasRating
{
    public uint TakenSeats { get; set; } = 0;

    public float Rating { get; set; }

    [MaxLength(256)]
    public string CoverImageId { get; set; } = string.Empty;

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public IList<string> ImageIds { get; set; }

    public List<TagDto> Tags { get; set; }

    public int NumberOfRatings { get; set; }

    [EnumDataType(typeof(WorkshopStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public WorkshopStatus Status { get; set; } = WorkshopStatus.Open;

    [JsonIgnore]
    public bool IsBlocked { get; set; }

    [EnumDataType(typeof(OwnershipType), ErrorMessage = Constants.EnumErrorMessage)]
    public OwnershipType ProviderOwnership { get; set; } = OwnershipType.State;

    [EnumDataType(typeof(ProviderStatus), ErrorMessage = Constants.EnumErrorMessage)]
    public ProviderStatus ProviderStatus { get; set; } = ProviderStatus.Pending;
    
    // TODO: for backward compatibility, remove when front changes
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string Website { get; set; } = string.Empty;

    public string Facebook { get; set; } = string.Empty;

    public string Instagram { get; set; } = string.Empty;

    public AddressDto Address { get; set; }

    [MaxLength(Constants.MaxLanguageNameLength)]
    public string LanguageOfEducationName { get; set; }
}

public static class WorkshopDtoExtensions
{
    public static WorkshopES ToES(this WorkshopDto dto)
    {
        var defaultContact = dto.Contacts?.FirstOrDefault(c => c.IsDefault);

        return new()
        {
            Id = dto.Id,
            Title = dto.Title,
            ShortTitle = dto.ShortTitle,
            CoverImageId = dto.CoverImageId,
            Rating = dto.Rating,
            NumberOfRatings = dto.NumberOfRatings,
            ProviderId = dto.ProviderId,
            ProviderTitle = dto.ProviderTitle,
            ProviderTitleEn = dto.ProviderTitleEn,
            ProviderStatus = dto.ProviderStatus,
            ProviderOwnership = dto.ProviderOwnership,
            Description = dto.WorkshopDescriptionItems
                ?.Aggregate(string.Empty, (accumulator, wdi) =>
                    $"{accumulator}{wdi.SectionName}{Constants.MappingSeparator}{wdi.Description}{Constants.MappingSeparator}"),
            MinAge = dto.MinAge ?? default,
            MaxAge = dto.MaxAge ?? default,
            CompetitiveSelection = dto.CompetitiveSelection,
            Price = dto.Price ?? default,
            PayRate = dto.PayRate ?? PayRateType.None,
            Address = defaultContact?.Address?.ToES(),
            InstitutionHierarchyId = dto.InstitutionHierarchyId,
            InstitutionHierarchy = dto.InstitutionHierarchy,
            InstitutionId = dto.InstitutionId,
            Institution = dto.Institution,
            Keywords = string.Join(Constants.MappingSeparator, dto.Keywords?.Distinct() ?? []),
            DirectionIds = dto.DirectionIds,
            SubDirectionIds = dto.SubDirectionIds,
            DateTimeRanges = dto.DateTimeRanges?.ToES() ?? [],
            Status = dto.Status,
            IsBlocked = dto.IsBlocked,
            AvailableSeats = dto.AvailableSeats ?? default,
            TakenSeats = dto.TakenSeats,
            ProviderLicenseStatus = dto.ProviderLicenseStatus,
            FormOfLearning = dto.FormOfLearning,
            AgeComposition = AgeComposition.SameAge, // not dto.AgeComposition - see original AM mapping,
            EducationalShift = EducationalShift.First, // not dto.EducationalShift - see original AM mapping,
            IsSelfFinanced = false, // not dto.IsSelfFinanced - see original AM mapping,
            IsPaid = dto.IsPaid,
            CompetitiveSelectionDescription = dto.CompetitiveSelectionDescription,
            SpecialNeedsType = SpecialNeedsType.None, // not dto.SpecialNeedsType - see original AM mapping,
            IsInclusive = false, // not dto.IsInclusive - see original AM mapping,
            EnrollmentProcedureDescription = dto.EnrollmentProcedureDescription,
            AreThereBenefits = dto.AreThereBenefits,
            PreferentialTermsOfParticipation = dto.PreferentialTermsOfParticipation,
            Coverage = dto.Coverage,
            Tags = dto.Tags?.Select(x => x.Name).ToList() ?? [],
            StudyPeriodStartDay = dto.StudyPeriodDates.StartDate.Day,
            StudyPeriodStartMonth = dto.StudyPeriodDates.StartDate.Month,
            StudyPeriodEndDay = dto.StudyPeriodDates.EndDate.Day,
            StudyPeriodEndMonth = dto.StudyPeriodDates.EndDate.Month,
            LanguageOfEducationId = dto.LanguageOfEducationId,
            LanguageOfEducationName = dto.LanguageOfEducationName,
            IsChampionPath =  dto.IsChampionPath,
        };
    }

    public static WorkshopDto ToDto(this Workshop model)
    {
        var defaultContact = model.Contacts?.FirstOrDefault(c => c.IsDefault);

        return new()
        {
            Id = model.Id,
            Title = model.Title,
            ShortTitle = model.ShortTitle,
            NoAgeRestrictions = model.MinAge == 0 && model.MaxAge == 120,
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
            WorkshopDescriptionItems = model.WorkshopDescriptionItems?.ToNotDeletedDto() ?? [],
            InstitutionId = model.InstitutionHierarchy?.InstitutionId,
            Institution = model.InstitutionHierarchy?.Institution?.Title,
            InstitutionHierarchyId = model.InstitutionHierarchyId,
            InstitutionHierarchy = model.InstitutionHierarchy?.Title,
            DefaultTeacher = model.DefaultTeacher?.ToDto(),
            DirectionIds = model.InstitutionHierarchy?.SubDirections?.Where(x => !x.IsDeleted).Select(d => d.DirectionId).ToList() ?? [],
            SubDirectionIds = model.InstitutionHierarchy?.SubDirections?.Where(sd => !sd.IsDeleted).Select(sd => sd.Id).ToList() ?? [],
            Keywords = model.Keywords?.Split(Constants.MappingSeparator, StringSplitOptions.None) ?? [],
            Teachers = model.Teachers?.ToNotDeletedDto() ?? [],
            ProviderId = model.ProviderId,
            ProviderTitle = model.ProviderTitle,
            ProviderTitleEn = model.ProviderTitleEn,
            ProviderLicenseStatus = model.Provider?.LicenseStatus ?? default,
            ActiveFrom = model.ActiveFrom,
            ActiveTo = model.ActiveTo,
            IsSelfFinanced = model.IsSelfFinanced,
            SpecialNeedsType = model.SpecialNeedsType,
            IsInclusive = model.IsInclusive,
            EnrollmentProcedureDescription = model.EnrollmentProcedureDescription,
            AreThereBenefits = model.AreThereBenefits,
            PreferentialTermsOfParticipation = model.PreferentialTermsOfParticipation,
            EducationalShift = model.EducationalShift,
            LanguageOfEducationId = model.LanguageOfEducationId,
            LanguageOfEducationName = model.LanguageOfEducation?.Name,
            StudyPeriodDates = model.ToStudyPeriodDatesDto(),
            AgeComposition = model.AgeComposition,
            Coverage = model.Coverage,
            WorkshopType = model.WorkshopType,
            DefaultTeacherId = model.DefaultTeacherId,
            ParentWorkshopId = model.ParentWorkshopId,
            ParentWorkshop = model.ParentWorkshop?.ToDto(),
            Contacts = model.Contacts?.ToDto(),

            TagIds = model.Tags?.Select(x => x.Id).ToList() ?? [],

            CoverImageId = model.CoverImageId,
            ImageIds = model.Images?.Select(x => x.ExternalStorageId).ToList() ?? [],
            Tags = model.Tags?.ToDto() ?? [],
            Status = model.Status,
            IsBlocked = model.Provider?.IsBlocked ?? default,
            ProviderOwnership = model.ProviderOwnership,
            ProviderStatus = model.Provider?.Status ?? default,
            Phone = defaultContact?.Phones?.FirstOrDefault()?.Number,
            Email = defaultContact?.Emails?.FirstOrDefault()?.Address,
            Website = defaultContact?.SocialNetworks?.FirstOrDefault(s => s.Type == SocialNetworkContactType.Website)?.Url,
            Facebook = defaultContact?.SocialNetworks?.FirstOrDefault(s => s.Type == SocialNetworkContactType.Facebook)?.Url,
            Instagram = defaultContact?.SocialNetworks?.FirstOrDefault(s => s.Type == SocialNetworkContactType.Instagram)?.Url,
            Address = defaultContact?.Address?.ToDto(),
            IsChampionPath = model.IsChampionPath,
        };
    }

    public static List<WorkshopDto> ToDto(this IEnumerable<Workshop> list)
        => list.MapToList(ToDto);
}
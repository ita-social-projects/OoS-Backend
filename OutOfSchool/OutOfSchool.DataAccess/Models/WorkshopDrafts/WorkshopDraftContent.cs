#nullable enable
using System;
using System.Collections.Generic;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models.WorkshopDrafts;

public class WorkshopDraftContent
{
    required public int MinAge { get; set; }

    required public int MaxAge { get; set; }

    required public uint EducationalShiftId { get; set; }

    required public bool ShortStay { get; set; }

    required public uint LanguageOfEducationId { get; set; }

    required public List<DateTimeRangeDraft> DateTimeRange { get; set; }

    required public List<WorkshopDescriptionItemDraft> WorkshopDescriptionItems { get; set; }

    required public bool IsSelfFinanced { get; set; }

    required public uint TotalSeats { get; set; }

    required public uint TypeOfAgeCompositionId { get; set; }

    required public bool CompetitiveSelection { get; set; }

    required public DateOnly ActiveFrom { get; set; }

    required public DateOnly ActiveTo { get; set; }

    required public List<Guid> EducationalDisciplinesId { get; set; }

    required public List<long> TagsIds { get; set; }

    required public string Title { get; set; }

    required public List<long> DirectionIds { get; set; }

    required public uint CategoryId { get; set; }

    required public uint GroupeTypeId { get; set; }

    required public uint CoverageId { get; set; }

    required public Guid MemberOfWorkshopId { get; set; }

    required public bool IsPaid { get; set; }

    required public bool IsSpecial { get; set; }

    required public bool IsInclusive { get; set; }

    required public List<string> Keywords { get; set; }

    public List<Guid>? IncludedStudyGroupsIds { get; set; }

    public uint? SpecialNeedsId { get; set; }

    public string? AdditionalDescription { get; set; }

    public string? ShortTitle { get; set; }

    public string? CompetitiveSelectionDescription { get; set; }

    public Guid? DefaultTeacherId { get; set; }

    public FormOfLearning? FormOfLearning { get; set; }

    public WorkshopStatus? WorkshopStatus { get; set; }

    public decimal? Price { get; set; }

    public PayRateType? PayRate { get; set; }

    public bool? AreThereBenefits { get; set; }

    public string? PreferentialTermsOfParticipation { get; set; }

    public Guid? InstitutionHierarchyId { get; set; }

    required public string Phone { get; set; }

    required public string Email { get; set; }

    public string? Website { get; set; }

    public string? Facebook { get; set; }

    public string? Instagram { get; set; }
}
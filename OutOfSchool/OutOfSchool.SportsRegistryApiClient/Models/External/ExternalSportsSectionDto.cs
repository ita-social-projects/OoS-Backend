using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OutOfSchool.SportsRegistryApiClient.Models.External;

public class ExternalSportsSectionDto
{
    public Guid SectionId { get; set; }
    
    public string SectionName { get; set; }
    public string SectionDescription { get; set; }

    public long SectionSportKindDictIdCode { get; set; }
    public string SectionSportKindDictName { get; set; }

    public int SectionAgeFrom { get; set; }
    public int SectionAgeTo { get; set; }

    public bool SectionIsInShlyahProject { get; set; }
    
    public string SectionAddressRegionDictIdCode { get; set; }
    public string SectionAddressRegionDictName { get; set; }
    public string SectionAddressDistrictDictIdCode { get; set; }
    public string SectionAddressDistrictDictName { get; set; }
    public string SectionAddressHromadaDictIdCode { get; set; }
    public string SectionAddressHromadaDictName { get; set; }
    public string SectionAddressLocalityDictIdCode { get; set; }
    public string SectionAddressLocalityDictName { get; set; }
    public string SectionAddressStreet { get; set; }
    public string SectionAddressHouse { get; set; }
    
    public string SectionEmail { get; set; }
    
    public string SectionPhones { get; set; }

    // URLs
    public string? SectionRegistrationFormUrl { get; set; }
    public string? SectionUrl { get; set; }
    public string? SectionFacebookUrl { get; set; }
    public string? SectionInstagramUrl { get; set; }
    
    public string SectionPracticeFormat { get; set; }
    public string SectionPracticeFormatDictName { get; set; }

    public decimal SectionPracticeCost { get; set; }
    public int SectionMaxStudentsAmount { get; set; }

    public DateTime SectionPracticePeriodDateFrom { get; set; }
    public DateTime SectionPracticePeriodDateTo { get; set; }
    
    public string SectionSchedule { get; set; }

    public string? SectionSelectionCriteria { get; set; }
    public string SectionRegistrationFlow { get; set; }
    
    public List<ExternalPhotoDto>? SectionPhotos { get; set; }
    public List<ExternalPhotoDto>? SectionTitlePhoto { get; set; }
    
    public string SectionTrainers { get; set; }
    
    public string SectionPozashkillyaModerationStatus { get; set; }
    public int SectionStatusDictIdCode { get; set; }
    public string SectionStatusDictName { get; set; }

    public string OrganizationCode { get; set; }
    
    public DateTime AddedToRegistryAt { get; set; }
    public DateTime UpdatedInRegistryAt { get; set; }
}

public class ExternalPhotoDto
{
    public Guid Id { get; set; }
    public string Checksum { get; set; }
}

public class ExternalScheduleItem
{
    [JsonPropertyName("sectionScheduleTimeFrom")]
    public string SectionScheduleTimeFrom { get; set; }

    [JsonPropertyName("sectionScheduleTimeTo")]
    public string SectionScheduleTimeTo { get; set; }

    [JsonPropertyName("section_schedule_weekday")]
    public string SectionScheduleWeekday { get; set; }

    [JsonPropertyName("sectionScheduleWeekdayDictName")]
    public string SectionScheduleWeekdayDictName { get; set; }
}
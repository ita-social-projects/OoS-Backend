using System.Text.Json.Serialization;

namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SportsSectionPostRequest
{
    [JsonPropertyName("organizationCode")]
    public string OrganizationCode { get; set; } = null!;

    [JsonPropertyName("sectionName")]
    public string SectionName { get; set; } = null!;

    [JsonPropertyName("sectionSportKindDictIdCode")]
    public int SectionSportKindDictIdCode { get; set; }

    [JsonPropertyName("sectionAgeFrom")]
    public int SectionAgeFrom { get; set; }

    [JsonPropertyName("sectionAgeTo")]
    public int SectionAgeTo { get; set; }

    [JsonPropertyName("sectionIsInShlyahProject")]
    public bool SectionIsInShlyahProject { get; set; }

    [JsonPropertyName("sectionAddressLocalityDictIdCode")]
    public string SectionAddressLocalityDictIdCode { get; set; } = null!;

    [JsonPropertyName("sectionAddressStreet")]
    public string SectionAddressStreet { get; set; } = null!;

    [JsonPropertyName("sectionAddressHouse")]
    public string SectionAddressHouse { get; set; } = null!;

    [JsonPropertyName("sectionDescription")]
    public string SectionDescription { get; set; } = null!;

    [JsonPropertyName("sectionRegistrationFlow")]
    public string SectionRegistrationFlow { get; set; } = null!;

    [JsonPropertyName("sectionPhones")]
    public List<string> SectionPhones { get; set; } = new();

    [JsonPropertyName("sectionEmail")]
    public string SectionEmail { get; set; } = null!;

    [JsonPropertyName("sectionRegistrationFormUrl")]
    public string SectionRegistrationFormUrl { get; set; } = null!;

    [JsonPropertyName("sectionUrl")]
    public string SectionUrl { get; set; } = null!;

    [JsonPropertyName("sectionFacebookUrl")]
    public string SectionFacebookUrl { get; set; } = null!;

    [JsonPropertyName("sectionInstagramUrl")]
    public string SectionInstagramUrl { get; set; } = null!;

    [JsonPropertyName("sectionPracticeFormat")]
    public string SectionPracticeFormat { get; set; } = null!;

    [JsonPropertyName("sectionSelectionCriteria")]
    public string SectionSelectionCriteria { get; set; } = null!;

    [JsonPropertyName("sectionPracticeCost")]
    public double SectionPracticeCost { get; set; }

    [JsonPropertyName("sectionMaxStudentsAmount")]
    public int SectionMaxStudentsAmount { get; set; }

    [JsonPropertyName("sectionTitlePhoto")]
    public string SectionTitlePhoto { get; set; } = null!;

    [JsonPropertyName("sectionPhotos")]
    public List<string> SectionPhotos { get; set; } = new();

    [JsonPropertyName("sectionTrainers")]
    public List<Guid> SectionTrainers { get; set; } = new();

    [JsonPropertyName("sectionSchedule")]
    public List<SectionScheduleRequest> SectionSchedule { get; set; } = new();
}

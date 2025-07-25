namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SportsSectionPostRequest
{
    public string OrganizationCode { get; set; } = null!;
    public string SectionName { get; set; } = null!;
    public int SectionSportKindDictIdCode { get; set; }
    public int SectionAgeFrom { get; set; }
    public int SectionAgeTo { get; set; }
    public bool SectionIsInShlyahProject { get; set; }
    public string SectionAddressLocalityDictIdCode { get; set; } = null!;
    public string SectionAddressStreet { get; set; } = null!;
    public string SectionAddressHouse { get; set; } = null!;
    public string SectionDescription { get; set; } = null!;
    public string SectionRegistrationFlow { get; set; } = null!;
    public List<string> SectionPhone { get; set; } = new();
    public string SectionEmail { get; set; } = null!;
    public string SectionRegistrationFormUrl { get; set; } = null!;
    public string SectionUrl { get; set; } = null!;
    public string SectionFacebookUrl { get; set; } = null!;
    public string SectionInstagramUrl { get; set; } = null!;
    public string SectionPracticeFormat { get; set; } = null!; // OFFLINE / ONLINE
    public string SectionSelectionCriteria { get; set; } = null!;
    public double SectionPracticeCost { get; set; }
    public int SectionMaxStudentsAmount { get; set; }
    public string SectionTitlePhoto { get; set; } = null!;
    public List<string> SectionPhotos { get; set; } = new();
    public List<Guid> SectionTrainers { get; set; } = new();
    public List<SectionScheduleRequest> SectionSchedule { get; set; } = new();
}
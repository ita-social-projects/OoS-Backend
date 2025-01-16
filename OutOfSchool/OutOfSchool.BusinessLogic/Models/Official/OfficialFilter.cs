namespace OutOfSchool.BusinessLogic.Models.Official;
public class OfficialFilter : OffsetFilter
{
    public string IndividualFirstName { get; set; } = string.Empty;
    public string IndividualMiddleName { get; set; } = string.Empty;
    public string IndividualLastName { get; set; } = string.Empty;
    public string IndividualRnokpp { get; set; } = string.Empty;
}

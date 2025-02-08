namespace OutOfSchool.AikomApiClient.Models.Data;

public class AikomProviderInfo
{
    public long Id { get; set; }

    public required string FullName { get; set; }

    public required string ShortName { get; set; }

    public string Edrpou { get; set; }

    public required string Address { get; set; }
}
namespace OutOfSchool.WebApi.Models.Providers;

public class ImportDataValidateRequest
{
    public Dictionary<int, string> Edrpous { get; set; } = new Dictionary<int, string>();

    public Dictionary<int, string> Emails { get; set; } = new Dictionary<int, string>();
}
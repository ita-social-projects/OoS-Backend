namespace OutOfSchool.WebApi.Models.Providers;

public class ImportDataValidateResponse
{
    public List<int> Edrpous { get; set; } = new List<int>();

    public List<int> Emails { get; set; } = new List<int>();
}

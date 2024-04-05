using CsvHelper.Configuration.Attributes;

namespace OutOfSchool.WebApi.Models.Providers;

public class ProviderCsvDto
{
    [Name("Назва закладу")]
    public string FullTitle { get; set; } = string.Empty;

    [Name("Форма власності")]
    public string Ownership { get; set; } = string.Empty;

    [Name("ЄДРПОУ / ІПН")]
    public string EdrpouIpn { get; set; } = string.Empty;

    [Name("Ліцензія №")]
    public string License { get; set; } = string.Empty;

    [Name("Населений пункт")]
    public string Settlement { get; set; } = string.Empty;

    [Name("Адреса")]
    public string Address { get; set; } = string.Empty;

    [Name("Електронна пошта")]
    public string Email { get; set; } = string.Empty;

    [Name("Телефон")]
    public string PhoneNumber { get; set; } = string.Empty;
}

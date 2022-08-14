namespace OutOfSchool.WebApi.Models;

public class MinistryAdminFilter : SearchStringFilter
{
    public Guid InstitutionId { get; set; } = Guid.Empty;
}
namespace OutOfSchool.WebApi.Models.Admins;

public class Ministry2AdminFilter : BaseAdminFilter
{
    public Guid InstitutionId { get; set; } = Guid.Empty;
}

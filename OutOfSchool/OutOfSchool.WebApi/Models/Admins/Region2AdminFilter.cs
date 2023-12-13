namespace OutOfSchool.WebApi.Models.Admins;

public class Region2AdminFilter : BaseAdminFilter
{
    public Guid InstitutionId { get; set; } = Guid.Empty;

    public long CATOTTGId { get; set; }

}

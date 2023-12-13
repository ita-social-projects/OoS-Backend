namespace OutOfSchool.WebApi.Models.Admins;

public class Region2AdminDto : BaseAdminDto
{
    public Guid InstitutionId { get; set; }

    public long CATOTTGId { get; set; }
}
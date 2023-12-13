namespace OutOfSchool.WebApi.Models.Admins;

public class Area2AdminDto : BaseAdminDto
{
    public Guid InstitutionId { get; set; }

    public long CATOTTGId { get; set; }

    public long RegionId { get; set; }
}
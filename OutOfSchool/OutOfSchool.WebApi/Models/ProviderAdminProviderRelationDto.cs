namespace OutOfSchool.WebApi.Models;

public class ProviderAdminProviderRelationDto
{
    public string UserId { get; set; }

    public Guid ProviderId { get; set; }

    public bool IsDeputy { get; set; }
}
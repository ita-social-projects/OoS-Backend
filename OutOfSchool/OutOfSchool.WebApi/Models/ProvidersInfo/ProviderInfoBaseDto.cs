


namespace OutOfSchool.WebApi.Models.ProvidersInfo;

public class ProviderInfoBaseDto
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    public List<WorkshopInfoBaseDto> Workshops { get; set; }
}

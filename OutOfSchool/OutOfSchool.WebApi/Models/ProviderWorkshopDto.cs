using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Models.Workshops;

namespace OutOfSchool.WebApi.Models;

public class ProviderWorkshopDto: ProviderDto
{
    public List<WorkshopDto> Workshops { get; set; }
}

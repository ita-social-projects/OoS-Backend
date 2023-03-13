namespace OutOfSchool.WebApi.Models;

public class FullProviderAdminDto : ProviderAdminDto
{
    public List<ShortEntityDto> WorkshopTitles { get; set; }
}

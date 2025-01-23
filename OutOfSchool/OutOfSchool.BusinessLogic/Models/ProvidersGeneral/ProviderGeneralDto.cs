using OutOfSchool.BusinessLogic.Models.Images;

namespace OutOfSchool.BusinessLogic.Models.ProvidersGeneral;
public class ProviderGeneralDto : ProviderGeneralBaseDto // for get method
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }

    public bool IsDeleted { get; set; }

    public List<ImageDto> Images { get; set; } = new List<ImageDto>();

    public Contact Contact { get; set; }

    public ProviderTypeDto ProviderType { get; set; }
}
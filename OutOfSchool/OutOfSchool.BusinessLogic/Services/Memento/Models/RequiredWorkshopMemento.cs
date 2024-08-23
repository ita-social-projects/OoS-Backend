using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Services.Memento.Models;

public class RequiredWorkshopMemento
{
    public string Title { get; set; } = string.Empty;

    public string ShortTitle { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int MinAge { get; set; }

    public int MaxAge { get; set; }

    public string ProviderTitle { get; set; } = string.Empty;

    public PayRateType PayRate { get; set; }

    public Guid ProviderId { get; set; }

    public long AddressId { get; set; }

    public FormOfLearning FormOfLearning { get; set; }
}
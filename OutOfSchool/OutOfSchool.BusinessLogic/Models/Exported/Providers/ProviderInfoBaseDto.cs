
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Exported.Providers;

[JsonDerivedType(typeof(ProviderInfoDto))]
public class ProviderInfoBaseDto : IExternalInfo<Guid>
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }
}

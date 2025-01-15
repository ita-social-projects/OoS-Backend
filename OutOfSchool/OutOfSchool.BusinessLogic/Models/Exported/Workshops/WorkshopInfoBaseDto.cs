using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Exported.Workshops;

[JsonDerivedType(typeof(WorkshopInfoDto))]
public class WorkshopInfoBaseDto : IExternalInfo<Guid>
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }
}

using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Exported.CompetitiveEvents;

[JsonDerivedType(typeof(CompetitiveEventInfoDto))]
public class CompetitiveEventInfoBaseDto: IExternalInfo<Guid>
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }
}
using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Exported.Directions;

[JsonDerivedType(typeof(DirectionInfoDto))]
public class DirectionInfoBaseDto : IExternalInfo<long>
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }
}
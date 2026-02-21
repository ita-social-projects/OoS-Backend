using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Exported.Directions;

[JsonDerivedType(typeof(SubDirectionsInfoDto))]
public class SubDirectionsInfoBaseDto : IExternalInfo<long>
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }
}
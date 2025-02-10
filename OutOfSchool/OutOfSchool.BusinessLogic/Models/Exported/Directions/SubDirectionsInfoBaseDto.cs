using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Models.Exported.Directions;

[JsonDerivedType(typeof(SubDirectionsInfoDto))]
public class SubDirectionsInfoBaseDto : IExternalInfo<Guid>
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }
}
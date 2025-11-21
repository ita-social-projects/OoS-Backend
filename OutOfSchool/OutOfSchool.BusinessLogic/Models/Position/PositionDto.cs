using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Position;
public class PositionDto // for get method
{
    public Guid Id { get; set; }

    public string Language { get; set; }

    public string Description { get; set; }

    public bool IsForRuralAreas { get; set; }

    public string Department { get; set; }

    public Guid? DepartmentId { get; set; }

    public int SeatsAmount { get; set; }

    public string FullName { get; set; }

    public string ShortName { get; set; }

    public string GenitiveName { get; set; }

    public bool IsTeachingPosition { get; set; }

    public float PositionRate { get; set; }

    public float Tariff { get; set; }

    public Guid PositionClassificationType { get; set; }

    public Guid ProviderId { get; set; }

    public Guid ContactId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }
    
    public PositionType PositionType { get; set; } = PositionType.Employee;

    public bool IsPedagogicalPosition { get; set; }

    public Guid PositionOpenedByOrganization { get; set; }

    public float TotalRatesForPosition { get; set; }

    public bool IsOffStaffPosition { get; set; }

    public bool IsDeleted { get; set; }
}

public static class PositionDtoExtensions
{
    public static PositionDto ToDto(this OutOfSchool.Services.Models.Position model)
        => new()
        {
            Id = model.Id,
            Language = model.Language,
            Description = model.Description,
            IsForRuralAreas = model.IsForRuralAreas,
            Department = model.Department,
            DepartmentId = model.DepartmentId,
            SeatsAmount = model.SeatsAmount,
            FullName = model.FullName,
            ShortName = model.ShortName,
            GenitiveName = model.GenitiveName,
            IsTeachingPosition = model.IsTeachingPosition,
            PositionRate = model.PositionRate,
            Tariff = model.Tariff,
            PositionClassificationType = model.PositionClassificationType,
            ProviderId = model.ProviderId,
            ContactId = model.ContactId,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(model.CreatedAt, DateTimeKind.Utc)),
            UpdatedAt = model.UpdatedAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(model.UpdatedAt.Value, DateTimeKind.Utc))
                : null,
            ActiveFrom = model.ActiveFrom,
            ActiveTo = model.ActiveTo,
            PositionType = model.PositionType,
            IsPedagogicalPosition = model.IsPedagogicalPosition,
            PositionOpenedByOrganization = model.PositionOpenedByOrganization,
            TotalRatesForPosition = model.TotalRatesForPosition,
            IsOffStaffPosition = model.IsOffStaffPosition,
            IsDeleted = model.IsDeleted,
        };

    public static List<PositionDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.Position> list)
        => list.MapToList(ToDto);
}

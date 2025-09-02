using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Position;
public class PositionDto // for get method
{
    public Guid Id { get; set; }

    public string Language { get; set; }

    public string Description { get; set; }

    public bool IsForRuralAreas { get; set; }

    public string Department { get; set; }

    public int SeatsAmount { get; set; }

    public string FullName { get; set; }

    public string ShortName { get; set; }

    public string GenitiveName { get; set; }

    public bool IsTeachingPosition { get; set; }

    public float Rate { get; set; }

    public float Tariff { get; set; }

    public string ClassifierType { get; set; }

    public Guid ProviderId { get; set; }

    public Guid ContactId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateOnly ActiveFrom { get; set; }

    public DateOnly ActiveTo { get; set; }
    
    public PositionType PositionType { get; set; } = PositionType.Employee;

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
            SeatsAmount = model.SeatsAmount,
            FullName = model.FullName,
            ShortName = model.ShortName,
            GenitiveName = model.GenitiveName,
            IsTeachingPosition = model.IsTeachingPosition,
            Rate = model.Rate,
            Tariff = model.Tariff,
            ClassifierType = model.ClassifierType,
            ProviderId = model.ProviderId,
            ContactId = model.ContactId,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(model.CreatedAt, DateTimeKind.Utc)),
            UpdatedAt = model.UpdatedAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(model.UpdatedAt.Value, DateTimeKind.Utc))
                : null,
            ActiveFrom = model.ActiveFrom,
            ActiveTo = model.ActiveTo,
            PositionType = model.PositionType,
            IsDeleted = model.IsDeleted,
        };

    public static List<PositionDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.Position> list)
        => list.MapToList(ToDto);
}

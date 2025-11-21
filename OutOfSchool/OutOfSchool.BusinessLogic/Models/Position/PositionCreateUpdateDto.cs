using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Models.Position;

public class PositionCreateUpdateDto
{
    [Required]
    [MaxLength(30)]
    public string Language { get; set; }

    [MaxLength(Constants.MaxPositionDescriptionLength)]
    public string Description { get; set; }

    [Required]
    [MaxLength(60)]
    public string Department { get; set; }

    public Guid? DepartmentId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "SeatsAmount must be non-negative.")]
    [DefaultValue(0)]
    public int SeatsAmount { get; set; }

    [Required]
    [MaxLength(Constants.NameMaxLength)]
    public string FullName { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    public string ShortName { get; set; }

    [Required]
    [MaxLength(Constants.NameMaxLength)]
    public string GenitiveName { get; set; }

    public bool IsTeachingPosition { get; set; }
    
    public bool IsForRuralAreas { get; set; }

    [Required]
    public float PositionRate { get; set; }

    [Required]
    public float Tariff { get; set; }

    [Required]
    public Guid PositionClassificationType { get; set; }

    [EnumDataType(typeof(PositionType), ErrorMessage = Constants.EnumErrorMessage)]
    public PositionType PositionType { get; set; } = PositionType.Employee;

    [Required]
    public bool? IsPedagogicalPosition { get; set; }

    [Required]
    public Guid PositionOpenedByOrganization { get; set; }

    [Required]
    public float TotalRatesForPosition { get; set; }

    [Required]
    public bool? IsOffStaffPosition { get; set; }
}

public static class PermissionsForRoleDTOExtensions
{
    public static OutOfSchool.Services.Models.Position ToModel(this PositionCreateUpdateDto dto)
        => new()
        {
            Language = dto.Language,
            Description = dto.Description,
            Department = dto.Department,
            DepartmentId = dto.DepartmentId,
            SeatsAmount = dto.SeatsAmount,
            FullName = dto.FullName,
            ShortName = dto.ShortName,
            GenitiveName = dto.GenitiveName,
            IsTeachingPosition = dto.IsTeachingPosition,
            IsForRuralAreas = dto.IsForRuralAreas,
            PositionRate = dto.PositionRate,
            Tariff = dto.Tariff,
            PositionClassificationType = dto.PositionClassificationType,
            PositionType = dto.PositionType,
            IsPedagogicalPosition = dto.IsPedagogicalPosition ?? false,
            PositionOpenedByOrganization = dto.PositionOpenedByOrganization,
            TotalRatesForPosition = dto.TotalRatesForPosition,
            IsOffStaffPosition = dto.IsOffStaffPosition ?? false,
        };

    public static OutOfSchool.Services.Models.Position SetToModel(this PositionCreateUpdateDto dto, OutOfSchool.Services.Models.Position model)
    {
        model.Language = dto.Language;
        model.Description = dto.Description;
        model.Department = dto.Department;
        model.DepartmentId = dto.DepartmentId;
        model.SeatsAmount = dto.SeatsAmount;
        model.FullName = dto.FullName;
        model.ShortName = dto.ShortName;
        model.GenitiveName = dto.GenitiveName;
        model.IsTeachingPosition = dto.IsTeachingPosition;
        model.IsForRuralAreas = dto.IsForRuralAreas;
        model.PositionRate = dto.PositionRate;
        model.Tariff = dto.Tariff;
        model.PositionClassificationType = dto.PositionClassificationType;
        model.PositionType = dto.PositionType;
        model.IsPedagogicalPosition = dto.IsPedagogicalPosition ?? false;
        model.PositionOpenedByOrganization = dto.PositionOpenedByOrganization;
        model.TotalRatesForPosition = dto.TotalRatesForPosition;
        model.IsOffStaffPosition = dto.IsOffStaffPosition ?? false;
        
        return model;
    }

    public static PositionCreateUpdateDto ToDirectorCreateDto(this OutOfSchool.Services.Models.Position position)
    {
        if (position == null)
        {
            throw new ArgumentNullException(nameof(position));
        }

        return new PositionCreateUpdateDto
        {
            FullName = "Директор ЗО",
            ShortName = "Директор",
            GenitiveName = "Директору",
            SeatsAmount = position.SeatsAmount,
            Language = position.Language,
            PositionRate = position.PositionRate,
            Tariff = position.Tariff,
            PositionClassificationType = position.PositionClassificationType,
            Department = position.Department,
            DepartmentId = position.DepartmentId,
            IsForRuralAreas = position.IsForRuralAreas,
            IsTeachingPosition = false,
            PositionType = PositionType.Director,
            Description = position.Description,
            IsPedagogicalPosition = position.IsPedagogicalPosition,
            PositionOpenedByOrganization = position.PositionOpenedByOrganization,
            TotalRatesForPosition = position.TotalRatesForPosition,
            IsOffStaffPosition = position.IsOffStaffPosition
        };
    }
}
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

    [Required]
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
    public float Rate { get; set; }

    [Required]
    public float Tariff { get; set; }

    [Required]
    [MaxLength(60)]
    public string ClassifierType { get; set; }

    [EnumDataType(typeof(PositionType), ErrorMessage = Constants.EnumErrorMessage)]
    public PositionType PositionType { get; set; } = PositionType.Employee;
}

public static class PermissionsForRoleDTOExtensions
{
    public static OutOfSchool.Services.Models.Position ToModel(this PositionCreateUpdateDto dto)
        => new()
        {
            Language = dto.Language,
            Description = dto.Description,
            Department = dto.Department,
            SeatsAmount = dto.SeatsAmount,
            FullName = dto.FullName,
            ShortName = dto.ShortName,
            GenitiveName = dto.GenitiveName,
            IsTeachingPosition = dto.IsTeachingPosition,
            IsForRuralAreas = dto.IsForRuralAreas,
            Rate = dto.Rate,
            Tariff = dto.Tariff,
            ClassifierType = dto.ClassifierType,
            PositionType = dto.PositionType,
        };

    public static OutOfSchool.Services.Models.Position SetToModel(this PositionCreateUpdateDto dto, OutOfSchool.Services.Models.Position model)
    {
        model.Language = dto.Language;
        model.Description = dto.Description;
        model.Department = dto.Department;
        model.SeatsAmount = dto.SeatsAmount;
        model.FullName = dto.FullName;
        model.ShortName = dto.ShortName;
        model.GenitiveName = dto.GenitiveName;
        model.IsTeachingPosition = dto.IsTeachingPosition;
        model.IsForRuralAreas = dto.IsForRuralAreas;
        model.Rate = dto.Rate;
        model.Tariff = dto.Tariff;
        model.ClassifierType = dto.ClassifierType;
        model.PositionType = dto.PositionType;
        
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
            Rate = position.Rate,
            Tariff = position.Tariff,
            ClassifierType = position.ClassifierType,
            Department = position.Department,
            IsForRuralAreas = position.IsForRuralAreas,
            IsTeachingPosition = false,
            PositionType = PositionType.Director,
            Description = position.Description
        };
    }
}
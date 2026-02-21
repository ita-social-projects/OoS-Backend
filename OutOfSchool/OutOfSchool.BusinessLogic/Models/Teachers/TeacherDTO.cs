using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Models;

public class TeacherDTO
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidFirstNameErrorMessage)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = Constants.RequiredLastNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidLastNameErrorMessage)]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidMiddleNameErrorMessage)]
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; } = default;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    public string CoverImageId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IFormFile CoverImage { get; set; }

    public Guid WorkshopId { get; set; }
}

public static class TeacherDTOExtensions
{
    public static Teacher SetToModel(this TeacherDTO dto, Teacher model)
    {
        model.FirstName = dto.FirstName;
        model.LastName = dto.LastName;
        model.MiddleName = dto.MiddleName ?? string.Empty;
        model.Gender = dto.Gender;
        model.DateOfBirth = dto.DateOfBirth;
        model.Description = dto.Description;

        return model;
    }

    public static Teacher ToModel(this TeacherDTO dto)
        => new()
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName ?? string.Empty,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Description = dto.Description,
            WorkshopId = dto.WorkshopId,
        };

    public static Teacher ToModel(this TeacherDTO dto, Guid id, Guid workshopId)
        => new()
        {
            Id = id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName ?? string.Empty,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Description = dto.Description,
            WorkshopId = workshopId,
        };

    public static List<Teacher> ToModel(this IEnumerable<TeacherDTO> list)
        => list.MapToList(ToModel);

    public static TeacherDraft ToDraft(this TeacherDTO dto)
        => new()
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName ?? string.Empty,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Description = dto.Description,
            CoverImageId = dto.CoverImageId,            
        };

    public static List<TeacherDraft> ToDraft(this IEnumerable<TeacherDTO> list)
        => list.MapToList(ToDraft);

    public static TeacherDTO ToDto(this Teacher model)
        => new()
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName ?? string.Empty,
            Gender = model.Gender,
            DateOfBirth = model.DateOfBirth,
            Description = model.Description,
            CoverImageId = model.CoverImageId,
            WorkshopId = model.WorkshopId ?? default,
        };

    public static TeacherDTO ToDto(this TeacherDraft draft)
        => new()
        {
            Id = draft.Id,
            FirstName = draft.FirstName,
            LastName = draft.LastName,
            MiddleName = draft.MiddleName ?? string.Empty,
            Gender = draft.Gender,
            DateOfBirth = draft.DateOfBirth,
            Description = draft.Description,
            CoverImageId = draft.CoverImageId,
        };

    public static TeacherDTO CopyDto(this TeacherDTO model)
        => new()
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName ?? string.Empty,
            Gender = model.Gender,
            DateOfBirth = model.DateOfBirth,
            Description = model.Description,
            CoverImageId = model.CoverImageId,
            CoverImage = model.CoverImage,
            WorkshopId = model.WorkshopId,
        };

    public static List<TeacherDTO> ToDto(this IEnumerable<Teacher> list)
        => list.MapToList(ToDto);

    public static List<TeacherDTO> ToDto(this IEnumerable<TeacherDraft> list)
        => list.MapToList(ToDto);

    public static List<TeacherDTO> ToNotDeletedDto(this IEnumerable<Teacher> list)
        => list.MapNonDeletedToList(ToDto);
}
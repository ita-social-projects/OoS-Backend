using Newtonsoft.Json;
using OutOfSchool.Common.Validators;
using OutOfSchool.Services.Enums;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.WorkshopDraft.TeacherDrafts;

public class TeacherDraftDto
{
    [Required(ErrorMessage = Constants.RequiredFirstNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidFirstNameErrorMessage)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = Constants.RequiredLastNameErrorMessage)]
    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidLastNameErrorMessage)]
    public string LastName { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(Constants.NameMaxLength)]
    [CustomUkrainianName(ErrorMessage = Constants.InvalidMiddleNameErrorMessage)]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [EnumDataType(typeof(Gender), ErrorMessage = Constants.EnumErrorMessage)]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [DataType(DataType.Text)]
    [MaxLength(Constants.TeacherDescriptionLength)]
    public string Description { get; set; }

    [Required]
    public bool IsDefaultTeacher { get; set; }
}
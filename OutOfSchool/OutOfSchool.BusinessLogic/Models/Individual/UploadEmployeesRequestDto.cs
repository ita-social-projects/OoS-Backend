using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.Individual;

public class UploadEmployeesRequestDto
{
    [Required(ErrorMessage = "Employee list for uploading is required")]
    [MinLength(1, ErrorMessage = "At least one employee must be provided")]
    [MaxLength(Constants.MaxNumberOfEmployeesToUpload, ErrorMessage = "Maximum number of employees per upload exceeded")]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public UploadEmployeeRequestDto[] Employees { get; set; }
}
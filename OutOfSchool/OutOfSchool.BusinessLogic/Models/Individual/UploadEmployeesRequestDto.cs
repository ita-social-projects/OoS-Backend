using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;

namespace OutOfSchool.BusinessLogic.Models.Individual;

public class UploadEmployeesRequestDto
{
    [Required(ErrorMessage = "Employee list for uploading is required")]
    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public UploadEmployeeRequestDto[] Employees { get; set; }
}
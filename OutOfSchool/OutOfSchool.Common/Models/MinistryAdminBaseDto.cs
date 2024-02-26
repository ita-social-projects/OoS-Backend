using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Models;

public class MinistryAdminBaseDto : AdminBaseDto
{
    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatingTime { get; set; }

    [Required(ErrorMessage = "InstitutionId is required")]
    public Guid InstitutionId { get; set; }
}
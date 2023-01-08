using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Models;

public class RegionAdminBaseDto : AdminBaseDto
{
    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatingTime { get; set; }

    [Required(ErrorMessage = "InstitutionId is required")]
    public Guid InstitutionId { get; set; }

    public string UserId { get; set; }

    [Required(ErrorMessage = "CATOTTGId is required")]
    public long CATOTTGId { get; set; }
}
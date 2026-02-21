using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Common.Models;

public class RegionAdminBaseDto : MinistryAdminBaseDto
{
    [Required(ErrorMessage = "CATOTTGId is required")]
    public long CATOTTGId { get; set; }
}
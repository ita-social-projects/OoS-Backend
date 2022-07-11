using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class PermissionsForRole : IKeyedEntity<long>
{
    public long Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string RoleName { get; set; }

    [Required]
    public string PackedPermissions { get; set; }

    [MaxLength(100)]
    public string Description { get; set; }
}
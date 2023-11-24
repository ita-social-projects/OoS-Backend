using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models;

public class PermissionsForRoleDTO
{
    public long Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string RoleName { get; set; }

    [Required]
    public IEnumerable<Permissions> Permissions { get; set; }

    [MaxLength(100)]
    public string Description { get; set; } = default;
}
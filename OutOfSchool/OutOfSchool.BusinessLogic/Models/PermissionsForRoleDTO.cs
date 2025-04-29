using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models;

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

public static class PermissionsForRoleDTOExtensions
{
    public static PermissionsForRole ToModel(this PermissionsForRoleDTO dto)
        => new()
        {
            Id = dto.Id,
            RoleName = dto.RoleName,
            PackedPermissions = dto.Permissions?.PackPermissionsIntoString(),
            Description = dto.Description,
        };

    public static PermissionsForRoleDTO ToDto(this PermissionsForRole model)
        => new()
        {
            Id = model.Id,
            RoleName = model.RoleName,
            Permissions = model.PackedPermissions?.UnpackPermissionsFromString(),
            Description = model.Description,
        };

    public static List<PermissionsForRoleDTO> ToDto(this IEnumerable<PermissionsForRole> list)
        => list.MapToList(ToDto);
}
using OutOfSchool.Common.PermissionsModule;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
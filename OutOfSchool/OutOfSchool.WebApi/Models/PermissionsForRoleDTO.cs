using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class PermissionsForRoleDTO
    {
        public long Id { get; set; }

        [Required]
        public string RoleName { get; set; }

        [Required]
        public string PackedPermissions { get; set; }

        public string Description { get; set; } = default;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OutOfSchool.Services.Models
{
    public class PermissionsForRole
    {
        public long Id { get; set; }

        [Required]
        public string RoleName { get; set; }

        [Required]
        public string PackedPermissions { get; set; }

        public string Description { get; set; }
    }
}

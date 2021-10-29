using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OutOfSchool.Services.Models
{
    public class PermissionsForRole
    {
        [Required]
        public string RoleName { get; set; } = default;
    }
}

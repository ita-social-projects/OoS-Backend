﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class PermissionsForRoleDTO
    {
        [Required]
        public string RoleName { get; set; } = default;
    }
}

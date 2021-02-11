﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Controllers
{
    public class RegisterViewModel
    {
        [Required]
        [MinLength(5)]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one capital, number and symbol.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords doesn't match")]
        public string ConfirmPassword { get; set; }
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set ; }
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"([0-9]{3})([-]?)([0-9]{3})([-]?)([0-9]{2})([-]?)([0-9]{2})", 
            ErrorMessage = "Phone number format is incorrect. Example: +38XXX-XXX-XX-XX")]
        public string PhoneNumber { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatingTime { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? LastLogin { get; set; }
        public string ReturnUrl { get; set; }
        [Required]
        public string UserRoleId { get; set; }  
        public List<IdentityRole> AllRoles { get; set; }

    }
}
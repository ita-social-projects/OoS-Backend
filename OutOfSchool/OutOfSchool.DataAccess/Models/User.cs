using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OutOfSchool.Services.Models
{
    public class User : IdentityUser
    {
        [DataType(DataType.DateTime)]
        public DateTimeOffset CreatingTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTimeOffset LastLogin { get; set; }

        [Required(ErrorMessage = "LastName is required")]
        [MaxLength(30)]
        public string LastName { get; set; }

        [MaxLength(30)]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "FirstName is required")]
        [MaxLength(30)]
        public string FirstName { get; set; }

        [MaxLength(50)]
        public string Role { get; set; }

        public bool IsRegistered { get; set; }
    }
}
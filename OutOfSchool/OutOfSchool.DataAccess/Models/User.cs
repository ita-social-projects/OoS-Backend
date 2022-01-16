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

        // If the flag is set, that user can no longer do anything to website.
        public bool IsEnabled { get; set; }

        // for permissions managing at login and check if user is original provider or its admin
        public bool IsDerived { get; set; } = false;
    }
}
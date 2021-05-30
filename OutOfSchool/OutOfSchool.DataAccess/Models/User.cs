using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OutOfSchool.Services.Models
{
    public class User : IdentityUser
    {
        [DataType(DataType.DateTime)]
        public DateTime CreatingTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastLogin { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string FirstName { get; set; }

        public string Role { get; set; }

        public bool IsRegistered { get; set; }
    }
}
using Microsoft.AspNetCore.Identity;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Models
{
    public class UserDto : IdentityUser
    {
        [DataType(DataType.DateTime)]
        public DateTime CreatingTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastLogin { get; set; }

        public string LastName { get; set; }

        public string MiddleName { get; set; }

        public string FirstName { get; set; }
    }
}

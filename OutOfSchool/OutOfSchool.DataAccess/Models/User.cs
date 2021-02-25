using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.Services.Models
{
    public class User : IdentityUser
    {
        [DataType(DataType.DateTime)]
        public DateTime CreatingTime { get; set; }
        
        [DataType(DataType.DateTime)]
        public DateTime? LastLogin { get; set; }
    }
}
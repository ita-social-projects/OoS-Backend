using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace OutOfSchool.Services.Models;

public class User : IdentityUser, IKeyedEntity<string>, ISoftDeleted
{
    public bool IsDeleted { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(60)]
    public string LastName { get; set; }

    [MaxLength(60)]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(60)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string Role { get; set; }

    // If the flag is true, that user can no longer do anything to website.
    public bool IsBlocked { get; set; } = false;
}
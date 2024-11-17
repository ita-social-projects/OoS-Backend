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

    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatingTime { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset LastLogin { get; set; }

    [MaxLength(60)]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(60)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string Role { get; set; }

    public bool IsRegistered { get; set; }

    // If the flag is true, that user can no longer do anything to website.
    public bool IsBlocked { get; set; } = false;

    // for permissions managing at login and check if user is original provider or its admin, temporary field, needs to be removed then
    public bool IsDerived { get; set; } = false;

    // If it's true then user must change his password before the logging into the system
    public bool MustChangePassword { get; set; }

    public virtual Individual? Individual { get; set; }
}
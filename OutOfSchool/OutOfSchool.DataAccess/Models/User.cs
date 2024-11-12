using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace OutOfSchool.Services.Models;
public class User : IdentityUser, IKeyedEntity<string>, ISoftDeleted
{
    public bool IsDeleted { get; set; }

    // TODO: For now it is left here so existing code does not break
    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(60)]
    public string LastName { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset LastLogin { get; set; }

    // TODO: For now it is left here so existing code does not break
    [MaxLength(60)]
    public string MiddleName { get; set; }

    // TODO: For now it is left here so existing code does not break
    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(60)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string Role { get; set; }

    // TODO: For now it is left here so existing code does not break
    public bool IsRegistered { get; set; }

    // If the flag is true, that user can no longer do anything to website.
    // If it's true then user must change his password before the logging into the system
    public bool MustChangePassword { get; set; }

    public virtual Individual? Individual { get; set; }
}
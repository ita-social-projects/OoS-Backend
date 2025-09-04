using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class Individual : BusinessEntity
{
    public bool IsRegistered { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(60)]
    public string FirstName { get; set; }

    [MaxLength(60)]
    public string MiddleName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(60)]
    public string LastName { get; set; }

    // TODO: Making rnokpp nullable was requested by MODT. This will affect authorization flow, so this needs to be fixed asap when they know what to do
    // [Required(ErrorMessage = "Rnokpp is required")]
    public string? Rnokpp { get; set; }

    // TODO: will be retrieved from aikom
    [Required(ErrorMessage = "ExternalRegistryId is required")]
    public Guid ExternalRegistryId { get; set; } = default;

    public Gender Gender { get; set; }

    public string? UserId { get; set; }

    public virtual User? User { get; set; }
    
    public virtual ICollection<Official> Officials { get; set; }
}
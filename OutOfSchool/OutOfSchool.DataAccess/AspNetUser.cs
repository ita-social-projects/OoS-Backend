using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services;

public class AspNetUser : BusinessEntity<User>
{
    // for permissions managing at login and check if user is original provider or its admin
    public bool IsDerived { get; set; } = false;

    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatingTime { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset LastLogin { get; set; }

    // If it's true then user must change his password before the logging into the system
    public bool MustChangePassword { get; set; }
}
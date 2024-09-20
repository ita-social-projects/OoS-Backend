using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services;

public class AspNetUser : BusinessEntity<User>
{
    public Guid Id { get; set; }

    public bool IsRegistered { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatingTime { get; set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset LastLogin { get; set; }
}
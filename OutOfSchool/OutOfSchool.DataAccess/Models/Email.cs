using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models;

public class Email:IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    [DataType(DataType.EmailAddress)]
    [MaxLength(Constants.MaxEmailAddressLength)]
    public string EmailAddress { get; set; }
}

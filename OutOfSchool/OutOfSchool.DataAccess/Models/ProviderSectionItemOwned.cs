using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;
[Owned]
public class ProviderSectionItemOwned
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }

    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

}

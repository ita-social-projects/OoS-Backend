﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class CompanyInformation : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Title { get; set; }

    [Required]
    public CompanyInformationType Type { get; set; }

    public virtual ICollection<CompanyInformationItem> CompanyInformationItems { get; set; }
}
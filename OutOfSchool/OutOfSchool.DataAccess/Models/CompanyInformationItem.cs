﻿using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class CompanyInformationItem : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string SectionName { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid CompanyInformationId { get; set; }

    public virtual CompanyInformation CompanyInformation { get; set; }
}
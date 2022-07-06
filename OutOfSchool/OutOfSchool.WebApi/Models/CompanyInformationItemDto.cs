﻿using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.WebApi.Models;

public class CompanyInformationItemDto
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string SectionName { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid CompanyInformationId { get; set; }
}
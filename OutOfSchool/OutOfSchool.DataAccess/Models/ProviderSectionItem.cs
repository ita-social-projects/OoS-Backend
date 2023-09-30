﻿using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models;

public class ProviderSectionItem : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [MaxLength(200)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string Description { get; set; }

    public Guid ProviderId { get; set; }

    public virtual Provider Provider { get; set; }
}
﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.Services.Models.Configurations;

internal class CompetitiveEventConfiguration : IEntityTypeConfiguration<CompetitiveEvent>
{
    public void Configure(EntityTypeBuilder<CompetitiveEvent> builder)
    {
        builder.ConfigureKeyedSoftDeleted<Guid, CompetitiveEvent>();
    }
}

﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.Services.Models.Configurations;

internal class CompetitiveEventDescriptionItemConfiguration : IEntityTypeConfiguration<CompetitiveEventDescriptionItem>
{
    public void Configure(EntityTypeBuilder<CompetitiveEventDescriptionItem> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}

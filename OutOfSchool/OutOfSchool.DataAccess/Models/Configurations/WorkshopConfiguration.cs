using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;

internal class WorkshopConfiguration : BusinessEntityConfiguration<Workshop>
{
    public override void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.HasMany(x => x.Employees)
            .WithMany(x => x.ManagedWorkshops);

        builder.HasMany(x => x.Teachers)
            .WithOne(x => x.Workshop)
            .HasForeignKey(x => x.WorkshopId);

        builder.HasMany(x => x.WorkshopDescriptionItems)
            .WithOne(x => x.Workshop)
            .HasForeignKey(x => x.WorkshopId);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(Constants.MaxWorkshopTitleLength);

        builder.Property(x => x.ShortTitle)
            .IsRequired()
            .HasMaxLength(Constants.MaxWorkshopShortTitleLength);

        base.Configure(builder);
    }
}

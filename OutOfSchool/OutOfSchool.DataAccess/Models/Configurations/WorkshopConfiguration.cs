﻿
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations
{
    internal class WorkshopConfiguration : IEntityTypeConfiguration<Workshop>
    {
        public void Configure(EntityTypeBuilder<Workshop> builder)
        {
            builder.HasOne(x => x.Direction)
                .WithMany()
                .IsRequired()
                .HasForeignKey(x => x.DirectionId)
                // Note: cascade delete causes circular dependency issue
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Provider)
                .WithMany(x => x.Workshops)
                // Note: cascade delete causes circular dependency issue
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

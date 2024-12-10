using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.Services.Models.Configurations;

internal class CompetitiveEventConfiguration : IEntityTypeConfiguration<CompetitiveEvent>
{
    public void Configure(EntityTypeBuilder<CompetitiveEvent> builder)
    {
        builder.ConfigureKeyedSoftDeleted<Guid, CompetitiveEvent>();
        builder.HasOne(c => c.Category)
            .WithMany() 
            .HasForeignKey(c => c.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CompetitiveEventAccountingType)
           .WithMany()
           .HasForeignKey(c => c.CompetitiveEventAccountingTypeId)
           .IsRequired(true)
           .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Judges)
           .WithOne(j => j.CompetitiveEvent)
           .HasForeignKey(j => j.CompetitiveEventId)
           .IsRequired(false)
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.CompetitiveEventDescriptionItems)
          .WithOne(d => d.CompetitiveEvent)
          .HasForeignKey(d => d.CompetitiveEventId)
          .IsRequired(false)
          .OnDelete(DeleteBehavior.Cascade);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;

internal class CompetitiveEventConfiguration : BusinessEntityWithContactsConfiguration<CompetitiveEvent>
{
    public override void Configure(EntityTypeBuilder<CompetitiveEvent> builder)
    {
        base.Configure(builder);

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

        builder.HasOne(c => c.InstitutionHierarchy)
           .WithMany()
           .HasForeignKey(c => c.InstitutionHierarchyId)
           .IsRequired(false)
           .OnDelete(DeleteBehavior.SetNull);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Common;
using OutOfSchool.Services.Enums.CompetitiveEventStatus;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.Configurations.BaseEntity;

namespace OutOfSchool.Services.Models.Configurations;
public class CompetitiveEventDraftConfiguration : TrackableBaseEntityConfiguration<CompetitiveEventDraft>
{
    public override void Configure(EntityTypeBuilder<CompetitiveEventDraft> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderId)
            .IsRequired();

        builder.Property(x => x.CoverImageId)
            .HasColumnType(ModelsConfigurationConstants.Char255Type);

        builder.Property(x => x.CompetitiveEventDraftContent)
            .IsRequired()
            .HasColumnType(ModelsConfigurationConstants.JsonType);

        builder.Property(x => x.DraftStatus)
            .HasDefaultValue(CompetitiveEventDraftStatus.Draft);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.CompetitiveEventDrafts)
            .HasForeignKey(x => x.ProviderId);

        builder.HasOne(x => x.CompetitiveEvent)
            .WithOne()
            .HasForeignKey<CompetitiveEventDraft>(x => x.CompetitiveEventId)
            .IsRequired(false);
    }
}

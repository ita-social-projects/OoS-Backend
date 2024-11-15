using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Enums.WorkshopStatus;
using OutOfSchool.Services.Models.Configurations.BaseEntity;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.Services.Models.Configurations.WorkshopDraftConfig;
public class WorkshopDraftConfiguration : TrackableBaseEntityConfiguration<WorkshopDraft>
{
    public override void Configure(EntityTypeBuilder<WorkshopDraft> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ProviderId)
            .IsRequired();

        builder.Property(x => x.CoverImageId)
            .HasColumnType("char(36)");

        builder.Property(x => x.WorkshopDraftContent)
            .IsRequired()
            .HasColumnType("json");

        builder.Property(x => x.DraftStatus)
            .HasDefaultValue(WorkshopDraftStatus.Draft);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasOne(x => x.Provider)
               .WithMany(x => x.WorkshopDrafts)
               .HasForeignKey(x => x.ProviderId);
    }
}
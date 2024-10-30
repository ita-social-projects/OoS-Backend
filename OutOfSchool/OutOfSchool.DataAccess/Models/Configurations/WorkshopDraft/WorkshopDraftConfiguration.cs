using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.Services.Models.Configurations.WorkshopDrafts;
internal class WorkshopDraftConfiguration : IEntityTypeConfiguration<WorkshopDraft>
{
    public void Configure(EntityTypeBuilder<WorkshopDraft> builder)
    {
        builder.ToTable("workshop_drafts");

        builder.HasKey(wd => wd.Id);

        builder.Property(wd => wd.CreatedAt)
            .IsRequired();

        builder.Property(wd => wd.UpdatedAt);

        builder.Property(wd => wd.CoverImageId)
            .HasColumnType("CHAR(36)")
            .IsRequired();

        builder.Property(wd => wd.WorkshopDraftContent)
            .HasColumnType("json");

        builder.HasOne(wd => wd.Provider)
            .WithMany(p => p.WorkshopDrafts)
            .HasForeignKey(wd => wd.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}

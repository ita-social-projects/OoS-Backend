using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.Services.Models.Configurations.WorkshopDrafts;
public class TeacherDraftConfiguration : IEntityTypeConfiguration<TeacherDraft>
{
    public void Configure(EntityTypeBuilder<TeacherDraft> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FirstName)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.MiddleName)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Gender)
            .IsRequired();

        builder.Property(x => x.DateOfBirth)
            .HasColumnType("DATE")
            .IsRequired();

        builder.Property(x => x.CoverImageId)
            .HasColumnType("char(36)");

        builder.Property(x => x.WorkshopDraftId)
            .IsRequired();

        builder.Property(x => x.IsDefaultTeacher)
            .HasDefaultValue(false);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasOne(x => x.WorkshopDraft)
           .WithMany(x => x.Teachers)
           .HasForeignKey(x => x.WorkshopDraftId);
    }
}

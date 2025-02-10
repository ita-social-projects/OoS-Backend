using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;
using OutOfSchool.Services.Common;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.Services.Models.Configurations.WorkshopDrafts;
public class TeacherDraftConfiguration : IEntityTypeConfiguration<TeacherDraft>
{
    public void Configure(EntityTypeBuilder<TeacherDraft> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(Constants.NameMaxLength);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(Constants.NameMaxLength);

        builder.Property(x => x.MiddleName)
            .IsRequired()
            .HasMaxLength(Constants.NameMaxLength);

        builder.Property(x => x.DateOfBirth)
            .IsRequired()
            .HasColumnType(ModelsConfigurationConstants.DateColumnType);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(ModelsConfigurationConstants.TeacherDescriptionCharacterLimit);

        builder.Property(x => x.CoverImageId)
            .HasColumnType(ModelsConfigurationConstants.Char36Type);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasOne(x => x.WorkshopDraft)
           .WithMany(x => x.Teachers)
           .HasForeignKey(x => x.WorkshopDraftId)
           .IsRequired();

        builder.Ignore(x => x.Images);
    }
}

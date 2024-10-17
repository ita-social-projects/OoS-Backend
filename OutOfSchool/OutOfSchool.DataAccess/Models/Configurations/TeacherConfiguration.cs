using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;
using OutOfSchool.Services.Common;

namespace OutOfSchool.Services.Models.Configurations;

internal class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

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
            .HasMaxLength(300); // ??

        builder.Ignore(x => x.Workshop);

        builder.Ignore(x => x.Images);
    }
}
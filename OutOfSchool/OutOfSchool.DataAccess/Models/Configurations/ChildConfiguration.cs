using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;
using OutOfSchool.Services.Common;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.Configurations;

internal class ChildConfiguration : IEntityTypeConfiguration<Child>
{
    public void Configure(EntityTypeBuilder<Child> builder)
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

        builder.Property(x => x.Gender)
            .IsRequired()
            .HasDefaultValue(Gender.Male);

        builder.Property(x => x.PlaceOfStudy)
            .HasMaxLength(500);
    }
}
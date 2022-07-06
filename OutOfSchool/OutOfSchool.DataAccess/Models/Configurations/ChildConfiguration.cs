using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OutOfSchool.Services.Common;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.Configurations;

internal class ChildConfiguration : IEntityTypeConfiguration<Child>
{
    public void Configure(EntityTypeBuilder<Child> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(ModelsConfigurationConstants.NameMaxLength);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(ModelsConfigurationConstants.NameMaxLength);

        builder.Property(x => x.MiddleName)
            .IsRequired()
            .HasMaxLength(ModelsConfigurationConstants.NameMaxLength);

        builder.Property(x => x.DateOfBirth)
            .IsRequired()
            .HasColumnType(ModelsConfigurationConstants.DateColumnType);

        builder.Property(x => x.Gender)
            .IsRequired()
            .HasDefaultValue(Gender.Male);

        builder.Property(x => x.PlaceOfStudy)
            .HasMaxLength(500);

        builder
            .HasMany(x => x.SocialGroups)
            .WithMany(x => x.Children)
            .UsingEntity<ChildSocialGroup>(
                j => j
                    .HasOne(pt => pt.SocialGroup)
                    .WithMany(t => t.ChildSocialGroups)
                    .HasForeignKey(pt => pt.SocialGroupId),
                j => j
                    .HasOne(pt => pt.Child)
                    .WithMany(t => t.ChildSocialGroups)
                    .HasForeignKey(pt => pt.ChildId));
    }
}
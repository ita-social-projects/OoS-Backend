using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;
using OutOfSchool.Services.Common;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.Services.Models.Configurations;
internal class JudgeConfiguration : IEntityTypeConfiguration<Judge>
{
    public void Configure(EntityTypeBuilder<Judge> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(Constants.NameMaxLength);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(Constants.NameMaxLength);

        builder.Property(x => x.MiddleName)
            .IsRequired(false)
            .HasMaxLength(Constants.NameMaxLength);

        builder.Property(x => x.DateOfBirth)
            .IsRequired()
            .HasColumnType(ModelsConfigurationConstants.DateColumnType);

        builder.Property(x => x.Gender)
            .IsRequired()
            .HasDefaultValue(Gender.Male);

        builder.Property(x => x.CoverImageId)
            .IsRequired(false);
    }
}
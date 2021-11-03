
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OutOfSchool.Services.Common;

namespace OutOfSchool.Services.Models.Configurations
{
    internal class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.HasKey(x => x.Id)
                .IsClustered(false);

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

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(300); // ??

            builder.Property(x => x.Image)
                .HasMaxLength(ModelsConfigurationConstants.ImageMaxLength);

            builder.HasOne(x => x.Workshop)
                .WithMany(x => x.Teachers)
                .HasForeignKey(x => x.WorkshopId)
                .IsRequired();
        }
    }
}

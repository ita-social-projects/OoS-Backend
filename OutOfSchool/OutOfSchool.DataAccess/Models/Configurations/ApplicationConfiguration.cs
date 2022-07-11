using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.Configurations;

internal class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasDefaultValue(ApplicationStatus.Pending);

        builder.Property(x => x.CreationTime)
            .IsRequired();
    }
}

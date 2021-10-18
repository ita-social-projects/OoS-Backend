
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.Configurations
{
    internal class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.HasKey(x => x.Id)
                .IsClustered(false);

            builder.Property(x => x.Status)
                .HasDefaultValue(ApplicationStatus.Pending);

            builder.Property(x => x.CreationTime)
                .IsRequired();

            builder.HasOne(x => x.Parent)
                .WithMany()
                // Note: cascade delete causes circular dependencie issue
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

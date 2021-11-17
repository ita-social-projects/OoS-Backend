using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations
{
    internal class ProviderAdminConfiguration : IEntityTypeConfiguration<ProviderAdmin>
    {
        public void Configure(EntityTypeBuilder<ProviderAdmin> builder)
        {
            builder.HasKey(x => x.UserId)
                .IsClustered(false);

            builder.Property(x => x.ProviderId)
                .IsRequired();

            // TODO:
            // DeleteBehavior.Cascade causes cycles or multiple cascade paths
            builder
                .HasOne(pa => pa.User)
                .WithOne(u => u.ProviderAdmin)
                .HasForeignKey<ProviderAdmin>(pa => pa.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

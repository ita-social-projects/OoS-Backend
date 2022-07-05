using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class ProviderAdminConfiguration : IEntityTypeConfiguration<ProviderAdmin>
{
    public void Configure(EntityTypeBuilder<ProviderAdmin> builder)
    {
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.ProviderId)
            .IsRequired();

        builder.Property(x => x.IsDeputy)
            .IsRequired();

        builder.HasMany(x => x.ManagedWorkshops)
            .WithMany(x => x.ProviderAdmins);
    }
}
}
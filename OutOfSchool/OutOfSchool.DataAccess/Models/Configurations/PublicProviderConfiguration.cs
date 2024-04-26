using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models.Configurations;
internal class PublicProviderConfiguration : IEntityTypeConfiguration<PublicProvider>
{
    public void Configure(EntityTypeBuilder<PublicProvider> builder)
    {
        builder.HasBaseType<Provider>();
        builder.Property(x => x.Status)
            .IsRequired()
        .HasDefaultValue(ProviderStatus.Pending);
        //builder.Property(x => x.StatusReason);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class SocialGroupConfiguration : IEntityTypeConfiguration<SocialGroup>
{
    public void Configure(EntityTypeBuilder<SocialGroup> builder)
    {
        builder.ConfigureKeyedSoftDeleted<long, SocialGroup>();
    }
}

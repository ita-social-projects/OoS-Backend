using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.Services.Models.Configurations;

internal class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}

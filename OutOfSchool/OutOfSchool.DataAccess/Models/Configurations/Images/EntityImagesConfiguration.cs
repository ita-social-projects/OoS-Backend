using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models.Configurations.Images;

internal class EntityImagesConfiguration<TEntity>(bool hasIndex = false) : IEntityTypeConfiguration<Image<TEntity>>
    where TEntity : class, IImageDependentEntity<TEntity>
{
    public void Configure(EntityTypeBuilder<Image<TEntity>> builder)
    {
        builder.HasKey(nameof(Image<TEntity>.EntityId), nameof(Image<TEntity>.ExternalStorageId));

        if (hasIndex)
        {
            builder.HasIndex(nameof(Image<TEntity>.ExternalStorageId));
        }

        builder
            .HasOne(x => x.Entity)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.EntityId);
    }
}
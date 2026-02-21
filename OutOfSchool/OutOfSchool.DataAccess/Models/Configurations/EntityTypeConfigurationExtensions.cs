using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

public static class EntityTypeConfigurationExtensions
{
    public static void ConfigureKeyedSoftDeleted<TKey, TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IKeyedEntity<TKey>, ISoftDeleted
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}

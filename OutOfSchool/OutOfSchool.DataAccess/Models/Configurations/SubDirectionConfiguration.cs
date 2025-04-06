using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;
public class SubDirectionConfiguration : IEntityTypeConfiguration<SubDirection>
{
    public void Configure(EntityTypeBuilder<SubDirection> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.HasOne(x => x.Direction)
            .WithMany(x => x.SubDirections)
            .HasForeignKey(x => x.DirectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.InstitutionHierarchies)
            .WithMany(x => x.SubDirections);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        // TODO: Reverse relationship, e.g., user knows about individual,
        // TODO: not individual about user?
        builder.HasOne(u => u.Individual)
            .WithOne(i => i.User)
            .HasForeignKey<Individual>(i => i.UserId)
            .IsRequired(false); // TODO: this is here so existing code won't break. Need to make required.
    }
}
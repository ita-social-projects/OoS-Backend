using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(x => x.UserId);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.Property(x => x.ProviderId)
            .IsRequired();

        builder.HasMany(x => x.ManagedWorkshops)
            .WithMany(x => x.Employees);
    }
}

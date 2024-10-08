using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;

internal class WorkshopConfiguration : BusinessEntityConfiguration<Workshop>
{
    public override void Configure(EntityTypeBuilder<Workshop> builder)
    {
        builder.HasMany(x => x.ProviderAdmins)
            .WithMany(x => x.ManagedWorkshops);

        builder.HasMany(x => x.WorkshopDescriptionItems)
             .WithOne(x => x.Workshop)
             .HasForeignKey(x => x.WorkshopId)
             .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.MemberOfWorkshop)
            .WithMany(x => x.IncludedStudyGroups)
            .HasForeignKey(x => x.MemberOfWorkshopId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-One relationship (with shadow property in Workshop)
        builder.HasOne(p => p.DefaultTeacher)
            .WithOne()
            .HasForeignKey<Workshop>(x => x.DefaultTeacherId) // Shadow property for One-to-One relationship
            .IsRequired(false) // Optional relationship
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-Many relationship (with shadow property in Workshop)
        builder.HasMany(p => p.Teachers)
            .WithOne()
            .HasForeignKey(x => x.WorkshopId) // Shadow property for One-to-Many relationship
            .IsRequired(false) // Optional relationship
            .OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
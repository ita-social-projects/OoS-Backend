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

        builder.HasMany(x => x.Teachers)
            .WithOne(x => x.Workshop)
            .HasForeignKey(x => x.WorkshopId);

        builder.HasMany(x => x.WorkshopDescriptionItems)
             .WithOne(x => x.Workshop)
             .HasForeignKey(x => x.WorkshopId)
             .OnDelete(DeleteBehavior.Cascade);

        // builder.Property(x => x.Title)
        //    .IsRequired()
        //    .HasMaxLength(Constants.MaxWorkshopTitleLength);

        // builder.Property(x => x.ShortTitle)
        //    .IsRequired()
        //    .HasMaxLength(Constants.MaxWorkshopShortTitleLength);

        builder.HasOne(x => x.DefaultTeacher)
            .WithOne(x => x.Workshop)
            .HasForeignKey<Teacher>(x => x.WorkshopId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MemberOfWorkshop)
            .WithMany(x => x.IncludedStudyGroups)
            .HasForeignKey(x => x.MemberOfWorkshopId)
            .OnDelete(DeleteBehavior.Restrict);

        base.Configure(builder);
    }
}
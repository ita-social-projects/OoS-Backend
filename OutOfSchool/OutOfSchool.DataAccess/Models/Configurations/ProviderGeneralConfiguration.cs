using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common;

namespace OutOfSchool.Services.Models.Configurations;
internal class ProviderGeneralConfiguration : IEntityTypeConfiguration<ProviderGeneral>
{
    public void Configure(EntityTypeBuilder<ProviderGeneral> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        builder.Property(x => x.BusinessName)
            .IsRequired()
            .HasMaxLength(Constants.MaxProviderFullTitleLength)
            .IsUnicode();

        builder.Property(x => x.FullTitle)
            .IsRequired()
            .HasMaxLength(Constants.MaxProviderFullTitleLength)
            .IsUnicode();

        builder.Property(x => x.ShortTitle)
            .IsRequired()
            .HasMaxLength(Constants.MaxProviderShortTitleLength)
            .IsUnicode();

        builder.Property(x => x.FullTitleEn)
            .HasMaxLength(Constants.MaxProviderFullTitleLength)
            .IsUnicode();

        builder.Property(x => x.ShortTitleEn)
            .HasMaxLength(Constants.MaxProviderShortTitleLength)
            .IsUnicode();

        builder.Property(x => x.Status)
            .HasDefaultValue(ProviderStatus.Pending);

        builder.Property(x => x.LicenseStatus)
            .IsRequired()
            .HasDefaultValue(ProviderLicenseStatus.NotProvided);

        builder.Property(x => x.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();

        builder.OwnsMany(x => x.SectionItems, sectionItem =>
        {
            sectionItem.Property(si => si.Id).IsRequired(); // PK
            sectionItem.Property(si => si.Name).HasMaxLength(200);
            sectionItem.Property(si => si.Description).HasMaxLength(2000);

            sectionItem.WithOwner().HasForeignKey(si => si.ProviderId); // Explicit FK
            sectionItem.HasKey(si => si.Id); // Explicit PK
        });
    }
}

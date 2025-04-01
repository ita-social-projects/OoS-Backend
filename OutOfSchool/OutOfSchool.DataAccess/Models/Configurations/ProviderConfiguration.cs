using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;

internal class ProviderConfiguration : BusinessEntityWithContactsConfiguration<Provider>
{
    public override void Configure(EntityTypeBuilder<Provider> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.ConfigureKeyedSoftDeleted<Guid, Provider>();

        builder.Property(x => x.FullTitle)
            .IsUnicode();

        builder.Property(x => x.ShortTitle)
            .IsUnicode();

        builder.Property(x => x.FullTitleEn)
            .IsUnicode();

        builder.Property(x => x.ShortTitleEn)
            .IsUnicode();

        builder.Property(x => x.Ownership)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.InstitutionType)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(ProviderStatus.Pending);

        builder.Property(x => x.LicenseStatus)
            .IsRequired()
            .HasDefaultValue(ProviderLicenseStatus.NotProvided);

        builder.HasIndex(x => x.Edrpou);

        builder.Property(x => x.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.Configurations;

internal class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullTitle)
            .IsRequired()
            .HasMaxLength(60) // Same as in short title. Bug ?
            .IsUnicode();

        builder.Property(x => x.ShortTitle)
            .IsRequired()
            .HasMaxLength(60) // Same as in full title. Bug ?
            .IsUnicode();

        builder.Property(x => x.Website)
            // TODO: use constant from ?? after url validation implementation
            .HasMaxLength(256)
            // Note: IDN
            .IsUnicode();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Facebook)
            // TODO: use constant from ?? after url validation implementation
            .HasMaxLength(256);

        builder.Property(x => x.Instagram)
            // TODO: use constant from ?? after url validation implementation
            .HasMaxLength(256);

        builder.Property(x => x.Director)
            .HasMaxLength(50)
            .IsUnicode();

        builder.Property(x => x.DirectorDateOfBirth)
            .HasColumnType(nameof(DataType.Date));

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(15);

        builder.Property(x => x.Founder)
            .IsRequired()
            .HasMaxLength(30);

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

        builder.HasOne(x => x.LegalAddress)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ActualAddress)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.EdrpouIpn);
    }
}

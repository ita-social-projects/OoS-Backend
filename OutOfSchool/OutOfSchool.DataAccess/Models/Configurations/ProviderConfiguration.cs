using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.Services.Models.Configurations;

internal class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

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

        builder.Property(x => x.Website)
            .HasMaxLength(Constants.MaxUnifiedUrlLength)
            .IsUnicode();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Facebook)
            .HasMaxLength(Constants.MaxUnifiedUrlLength);

        builder.Property(x => x.Instagram)
            .HasMaxLength(Constants.MaxUnifiedUrlLength);

        builder.Property(x => x.Director)
            .HasMaxLength(50)
            .IsUnicode();

        builder.Property(x => x.DirectorDateOfBirth)
            .HasColumnType(nameof(DataType.Date));

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(Constants.MaxPhoneNumberLengthWithPlusSign);

        builder.Property(x => x.Founder)
            .IsRequired()
            .HasMaxLength(Constants.MaxProviderFounderLength);

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

        builder.Property(x => x.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();
    }
}

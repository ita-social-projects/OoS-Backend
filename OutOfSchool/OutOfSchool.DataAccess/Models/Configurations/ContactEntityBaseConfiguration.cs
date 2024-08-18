using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.Configurations;

internal class ContactEntityBaseConfiguration : IEntityTypeConfiguration<ContactEntityBase>
{
    public void Configure(EntityTypeBuilder<ContactEntityBase> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasDiscriminator(x => x.Type)
            .HasValue<Instagram>(ContactInformationType.Instagram)
            .HasValue<Facebook>(ContactInformationType.Facebook)
            .HasValue<Website>(ContactInformationType.Website)
            .HasValue<PhoneNumber>(ContactInformationType.PhoneNumber)
            .HasValue<Email>(ContactInformationType.Email);
    }
}

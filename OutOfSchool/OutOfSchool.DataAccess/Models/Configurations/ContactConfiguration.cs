using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace OutOfSchool.Services.Models.Configurations;

internal class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.OptionalContacts)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<OptionalContacts>(v))
                .HasColumnName(nameof(OptionalContacts))
                .HasColumnType("longtext");

        builder.Property(e => e.PhoneNumbers)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<PhoneNumber>>(v))
                .HasColumnName(nameof(PhoneNumber))
                .HasColumnType("longtext");
    }
}

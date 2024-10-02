using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class ContactInformationConfiguration
{
    public void Configure(EntityTypeBuilder<ContactInformation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.Addresses)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

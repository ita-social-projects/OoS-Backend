using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class CodeficatorConfiguration : IEntityTypeConfiguration<CATOTTG>
{
    public void Configure(EntityTypeBuilder<CATOTTG> builder)
    {
        builder
            .Property<bool>("IsTop");
    }
}
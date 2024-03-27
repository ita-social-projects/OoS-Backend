using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

internal class QuartzJobConfiguration : IEntityTypeConfiguration<QuartzJob>
{
    public void Configure(EntityTypeBuilder<QuartzJob> builder)
    {
        builder.HasKey(x => x.Id);
    }
}

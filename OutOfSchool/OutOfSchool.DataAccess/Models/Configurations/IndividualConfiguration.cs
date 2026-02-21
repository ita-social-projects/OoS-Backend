using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;

internal class IndividualConfiguration : BusinessEntityConfiguration<Individual>
{
    public override void Configure(EntityTypeBuilder<Individual> entityTypeBuilder)
    {
        base.Configure(entityTypeBuilder);
        entityTypeBuilder.HasIndex(i => i.Rnokpp).IsUnique();
    }
}
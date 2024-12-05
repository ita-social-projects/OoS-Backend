using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;

internal class IndividualConfiguration : BusinessEntityConfiguration<Individual>
{
    public override void Configure(EntityTypeBuilder<Individual> entityTypeBuilder)
    {
        entityTypeBuilder.HasIndex(i => i.Rnokpp).IsUnique();
        base.Configure(entityTypeBuilder);
    }
}
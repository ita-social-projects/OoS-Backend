using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.Services.Models.Configurations;

internal class CompetitiveEventAccountingTypeConfiguration : IEntityTypeConfiguration<CompetitiveEventAccountingType>
{
    public void Configure(EntityTypeBuilder<CompetitiveEventAccountingType> builder)
    {
        builder.ConfigureKeyedSoftDeleted<int, CompetitiveEventAccountingType>();
    }
}
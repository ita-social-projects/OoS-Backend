using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.Services.Models.Configurations;

internal class CompetitiveEventCoverageConfiguration : IEntityTypeConfiguration<CompetitiveEventCoverage>
{
    public void Configure(EntityTypeBuilder<CompetitiveEventCoverage> builder)
    {
        builder.ConfigureKeyedSoftDeleted<int, CompetitiveEventCoverage>();
    }
}

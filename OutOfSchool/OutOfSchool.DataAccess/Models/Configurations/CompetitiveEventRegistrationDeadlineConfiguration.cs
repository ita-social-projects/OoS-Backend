using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.Services.Models.Configurations;

internal class CompetitiveEventRegistrationDeadlineConfiguration : IEntityTypeConfiguration<CompetitiveEventRegistrationDeadline>
{
    public void Configure(EntityTypeBuilder<CompetitiveEventRegistrationDeadline> builder)
    {
        builder.ConfigureKeyedSoftDeleted<int, CompetitiveEventRegistrationDeadline>();
    }
}

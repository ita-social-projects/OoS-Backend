using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;
public class SubjectConfiguration : IEntityTypeConfiguration<StudySubject>
{
    public void Configure(EntityTypeBuilder<StudySubject> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.IsDeleted);

        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        //TO DO
    }
}

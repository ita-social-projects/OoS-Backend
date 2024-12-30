using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;
public class StudySubjectLanguageConfiguration : IEntityTypeConfiguration<StudySubjectLanguage>
{
    public void Configure(EntityTypeBuilder<StudySubjectLanguage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.StudySubject)
            .WithMany(x => x.StudySubjectLanguages)
            .HasForeignKey(x => x.StudySubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Language)
            .WithMany(x => x.StudySubjectLanguages)
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

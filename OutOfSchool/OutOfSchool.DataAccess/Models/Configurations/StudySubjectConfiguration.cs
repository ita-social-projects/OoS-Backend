using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;
public class StudySubjectConfiguration : BusinessEntityConfiguration<StudySubject>
{
    public override void Configure(EntityTypeBuilder<StudySubject> builder)
    {
        builder.Property(x => x.NameInUkrainian)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.NameInInstructionLanguage)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsPrimaryLanguageUkrainian)
            .IsRequired();

        builder.HasOne(x => x.PrimaryLanguage)
            .WithMany(x => x.StudySubjects)
            .HasForeignKey(x => x.PrimaryLanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Workshop)
            .WithMany(x => x.StudySubjects)
            .HasForeignKey(x => x.WorkshopId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

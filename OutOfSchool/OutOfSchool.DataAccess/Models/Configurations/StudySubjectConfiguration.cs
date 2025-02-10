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

        builder.Property(x => x.IsLanguageUkrainian)
            .IsRequired();

        builder.HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        base.Configure(builder);
    }
}

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
            .WithMany()
            .HasForeignKey(x => x.PrimaryLanguageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Languages)
            .WithMany();

        base.Configure(builder);
    }
}

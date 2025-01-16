using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models;

public class OfficialConfiguration: IEntityTypeConfiguration<Official>
{
    public void Configure(EntityTypeBuilder<Official> builder)
    {
        builder.HasOne(o => o.Individual)
            .WithMany(i => i.Officials)
            .HasForeignKey(o => o.IndividualId);

        builder.HasOne(o => o.Position)
            .WithMany(p => p.Officials)
            .HasForeignKey(o => o.PositionId);
        
        builder.Property(o => o.PositionId).IsRequired();
        
        builder.Property(o => o.IndividualId).IsRequired();
        
        builder.Property(o => o.RecruitmentOrder).HasMaxLength(2000);
        
        builder.Property(o => o.DismissalOrder).HasMaxLength(2000);
        
        builder.Property(o => o.DismissalReason).HasMaxLength(255); 
    }
}
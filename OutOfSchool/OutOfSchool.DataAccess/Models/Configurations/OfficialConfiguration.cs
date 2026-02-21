using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;

public class OfficialConfiguration: BusinessEntityConfiguration<Official>
{
    public override void Configure(EntityTypeBuilder<Official> builder)
    {
        base.Configure(builder);

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
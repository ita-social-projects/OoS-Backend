using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Common;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models.Configurations.Base;

namespace OutOfSchool.Services.Models.Configurations;

public class PositionConfiguration: BusinessEntityConfiguration<Position>
{
    public override void Configure(EntityTypeBuilder<Position> builder)
    {
        base.Configure(builder);

        builder.HasOne(p => p.Provider)
            .WithMany(pr => pr.Positions)
            .HasForeignKey(p => p.ProviderId);
        
        builder.Property(p => p.Language).HasMaxLength(30);
        
        builder.Property(p => p.Description).HasMaxLength(Constants.MaxPositionDescriptionLength);
        
        builder.Property(p => p.Department).HasMaxLength(60);
        
        builder.Property(p => p.SeatsAmount).IsRequired();
        
        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(Constants.NameMaxLength);
        
        builder.Property(p => p.ShortName)
            .HasMaxLength(Constants.NameMaxLength);
        
        builder.Property(p => p.GenitiveName)
            .IsRequired()
            .HasMaxLength(Constants.NameMaxLength);
        
        builder.Property(p => p.PositionRate).IsRequired();
        
        builder.Property(p => p.Tariff).IsRequired();
        
        builder.Property(p => p.PositionClassificationType).IsRequired();

        builder.Property(p => p.PositionType)
            .HasDefaultValue(PositionType.Employee);

        builder.Property(p => p.IsPedagogicalPosition).IsRequired();

        builder.Property(p => p.PositionOpenedByOrganization).IsRequired();

        builder.Property(p => p.TotalRatesForPosition).IsRequired();

        builder.Property(p => p.IsOffStaffPosition).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_Positions_SeatsAmount_NonNegative", "SeatsAmount >= 0"));
    }
}
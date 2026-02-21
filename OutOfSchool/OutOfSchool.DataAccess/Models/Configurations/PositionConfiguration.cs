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
        
        builder.Property(p => p.Rate).IsRequired();
        
        builder.Property(p => p.Tariff).IsRequired();
        
        builder.Property(p => p.ClassifierType)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(p => p.PositionType)
            .HasDefaultValue(PositionType.Employee);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

public class CompanyInformationItemConfiguration : IEntityTypeConfiguration<CompanyInformationItem>
{
    public void Configure(EntityTypeBuilder<CompanyInformationItem> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.HasKey(x => x.Id);
    }
}
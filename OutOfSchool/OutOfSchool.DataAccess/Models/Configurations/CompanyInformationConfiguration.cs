using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

public class CompanyInformationConfiguration :IEntityTypeConfiguration<CompanyInformation>
{
    public void Configure(EntityTypeBuilder<CompanyInformation> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.HasKey(x => x.Id);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OutOfSchool.Services.Models.Configurations;

public class ElasticsearchSyncRecordConfiguration : IEntityTypeConfiguration<ElasticsearchSyncRecord>
{
    public void Configure(EntityTypeBuilder<ElasticsearchSyncRecord> builder)
    {
        builder.Property(x => x.Id).HasColumnType("UUID");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.RecordId).HasColumnType("UUID");
    }
}
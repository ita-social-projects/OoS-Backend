using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services;
using OutOfSchool.Services.Models.CompetitiveEventDrafts;
using OutOfSchool.Services.Models.WorkshopDrafts;
using System.Text.Json;

namespace OutOfSchool.Tests.Common.DbContextTests;
public class TestOutOfSchoolDbContext : OutOfSchoolDbContext
{
    public TestOutOfSchoolDbContext(DbContextOptions<OutOfSchoolDbContext> options) 
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<WorkshopDraft>(entity =>
        {
            entity.Property(p => p.WorkshopDraftContent)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<WorkshopDraftContent>(v, (JsonSerializerOptions)null));
        });

        builder.Entity<CompetitiveEventDraft>(entity =>
        {
            entity.Property(p => p.CompetitiveEventDraftContent)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<CompetitiveEventDraftContent>(v, (JsonSerializerOptions)null));
        });
    }
}

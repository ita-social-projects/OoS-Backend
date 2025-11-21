using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.Configurations.Base;
using OutOfSchool.Services.Common;
using OutOfSchool.Common;
using System.Collections.Generic;

namespace OutOfSchool.Services.Models.Configurations;

public class DepartmentConfiguration : BusinessEntityWithContactsConfiguration<Department>
{
    public override void Configure(EntityTypeBuilder<Department> builder)
    {
        base.Configure(builder);
        
        builder.Property(d => d.Description).HasMaxLength(OutOfSchool.Common.Constants.MaxPositionDescriptionLength);
        builder.Property(d => d.ShortName).HasMaxLength(OutOfSchool.Common.Constants.NameMaxLength);
        builder.Property(d => d.FullName).IsRequired().HasMaxLength(OutOfSchool.Common.Constants.NameMaxLength);
        builder.Property(d => d.DepartmentType).IsRequired();
        builder.Property(d => d.ParentDepartmentId).HasColumnType("UUID");
        builder.Property(d => d.ParticipantId).HasColumnType("UUID");
        
        builder.Property(d => d.ParentOrganizationId).IsRequired().HasColumnType("UUID");
        builder.Property(d => d.EducationProcessForm);
        builder.Property(d => d.EducationalDirections)
            .HasConversion(
                v => JsonSerializerHelper.Serialize(v, null),
                v => JsonSerializerHelper.Deserialize<List<string>>(v, null) ?? new List<string>())
            .HasColumnType(ModelsConfigurationConstants.JsonType);
        builder.Property(d => d.EducationLevelProvided)
            .HasConversion(
                v => JsonSerializerHelper.Serialize(v, null),
                v => JsonSerializerHelper.Deserialize<List<string>>(v, null) ?? new List<string>())
            .HasColumnType(ModelsConfigurationConstants.JsonType);
        builder.Property(d => d.OrganizationSpecialization)
            .HasConversion(
                v => JsonSerializerHelper.Serialize(v, null),
                v => JsonSerializerHelper.Deserialize<List<string>>(v, null) ?? new List<string>())
            .HasColumnType(ModelsConfigurationConstants.JsonType);
        builder.Property(d => d.HasConsultationUnit);
        builder.Property(d => d.GeneralSchedule);
        builder.Property(d => d.AdditionalDescription);
        builder.Property(d => d.OperationalStatus);
        builder.Property(d => d.IsLocatedInMountains).IsRequired();
        builder.Property(d => d.IsBranchUnit);
        builder.Property(d => d.ZpoType);
    }
}

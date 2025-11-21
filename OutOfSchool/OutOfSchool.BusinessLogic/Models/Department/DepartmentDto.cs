using OutOfSchool.BusinessLogic.Models.ContactInfo;

namespace OutOfSchool.BusinessLogic.Models.Department;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Description { get; set; }
    public string ShortName { get; set; }
    public string GenitiveName { get; set; }
    public string FullName { get; set; }
    public string Abbreviation { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string DepartmentType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateOnly ActiveTo { get; set; }
    public DateOnly ActiveFrom { get; set; }
    public bool IsDeleted { get; set; }
    public List<ContactsDto> Contacts { get; set; }
    public Guid ParentOrganizationId { get; set; }
    public string EducationProcessForm { get; set; }
    public List<string> EducationalDirections { get; set; }
    public List<string> EducationLevelProvided { get; set; }
    public List<string> OrganizationSpecialization { get; set; }
    public bool HasConsultationUnit { get; set; }
    public string GeneralSchedule { get; set; }
    public string AdditionalDescription { get; set; }
    public string OperationalStatus { get; set; }
    public bool IsLocatedInMountains { get; set; }
    public bool IsBranchUnit { get; set; }
    public string ZpoType { get; set; }

}

public static class DepartmentDtoExtensions
{
    public static DepartmentDto ToDto(this OutOfSchool.Services.Models.Department model)
        => new()
        {
            Id = model.Id,
            Description = model.Description,
            ShortName = model.ShortName,
            GenitiveName = model.GenitiveName,
            FullName = model.FullName,
            Abbreviation = model.Abbreviation,
            ParentDepartmentId = model.ParentDepartmentId,
            DepartmentType = model.DepartmentType,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(model.CreatedAt, DateTimeKind.Utc)),
            UpdatedAt = model.UpdatedAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(model.UpdatedAt.Value, DateTimeKind.Utc))
                : null,
            ActiveFrom = model.ActiveFrom,
            ActiveTo = model.ActiveTo,
            IsDeleted = model.IsDeleted,
            Contacts = model.Contacts?.ToDto() ?? [],
            ParentOrganizationId = model.ParentOrganizationId,
            EducationProcessForm = model.EducationProcessForm,
            EducationalDirections = model.EducationalDirections ?? [],
            EducationLevelProvided = model.EducationLevelProvided ?? [],
            OrganizationSpecialization = model.OrganizationSpecialization ?? [],
            HasConsultationUnit = model.HasConsultationUnit,
            GeneralSchedule = model.GeneralSchedule,
            AdditionalDescription = model.AdditionalDescription,
            OperationalStatus = model.OperationalStatus,
            IsLocatedInMountains = model.IsLocatedInMountains,
            IsBranchUnit = model.IsBranchUnit,
            ZpoType = model.ZpoType,
        };

    public static List<DepartmentDto> ToDto(this IEnumerable<OutOfSchool.Services.Models.Department> list)
        => list.MapToList(ToDto);
}
using OutOfSchool.BusinessLogic.Models.ContactInfo;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Util.JsonTools;
using OutOfSchool.BusinessLogic.Util.CustomValidation;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.BusinessLogic.Models.Department;

public class DepartmentCreateUpdateDto
{
    [MaxLength(Constants.MaxPositionDescriptionLength)]
    public string Description { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    public string ShortName { get; set; }

    public string GenitiveName { get; set; }

    [Required(ErrorMessage = "FullName is required.")]
    [MaxLength(Constants.NameMaxLength)]
    public string FullName { get; set; }

    public string Abbreviation { get; set; }

    public Guid? ParentDepartmentId { get; set; }

    [Required(ErrorMessage = "DepartmentType is required.")]
    public string DepartmentType { get; set; }

    [Required(ErrorMessage = "ParentOrganizationId is required.")]
    public Guid ParentOrganizationId { get; set; }

    public string EducationProcessForm { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<string> EducationalDirections { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<string> EducationLevelProvided { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    public List<string> OrganizationSpecialization { get; set; }

    public bool HasConsultationUnit { get; set; }

    public string GeneralSchedule { get; set; }

    public string AdditionalDescription { get; set; }

    public string OperationalStatus { get; set; }

    [Required(ErrorMessage = "IsLocatedInMountains is required.")]
    public bool? IsLocatedInMountains { get; set; }

    public bool IsBranchUnit { get; set; }

    public string ZpoType { get; set; }

    [ModelBinder(BinderType = typeof(JsonModelBinder))]
    [ContactsMustHavePhones(ErrorMessage = "Each contact must have at least one phone number.")]
    public List<ContactsDto> Contacts { get; set; }
}

public static class PermissionsForRoleDTOExtensions
{
    public static OutOfSchool.Services.Models.Department ToModel(this DepartmentCreateUpdateDto dto)
        => new()
        {
            Description = dto.Description,
            ShortName = dto.ShortName,
            GenitiveName = dto.GenitiveName,
            FullName = dto.FullName,
            Abbreviation = dto.Abbreviation,
            ParentDepartmentId = dto.ParentDepartmentId,
            DepartmentType = dto.DepartmentType,
            ParentOrganizationId = dto.ParentOrganizationId,
            EducationProcessForm = dto.EducationProcessForm,
            EducationalDirections = dto.EducationalDirections ?? [],
            EducationLevelProvided = dto.EducationLevelProvided ?? [],
            OrganizationSpecialization = dto.OrganizationSpecialization ?? [],
            HasConsultationUnit = dto.HasConsultationUnit,
            GeneralSchedule = dto.GeneralSchedule,
            AdditionalDescription = dto.AdditionalDescription,
            OperationalStatus = dto.OperationalStatus,
            IsLocatedInMountains = dto.IsLocatedInMountains ?? throw new InvalidOperationException("IsLocatedInMountains is required."),
            IsBranchUnit = dto.IsBranchUnit,
            ZpoType = dto.ZpoType,
            Contacts = dto.Contacts?.ToModel() ?? [],
        };

    public static OutOfSchool.Services.Models.Department SetToModel(this DepartmentCreateUpdateDto dto, OutOfSchool.Services.Models.Department model)
        {
            model.Description = dto.Description;
            model.ShortName = dto.ShortName;
            model.GenitiveName = dto.GenitiveName;
            model.FullName = dto.FullName;
            model.Abbreviation = dto.Abbreviation;
            model.ParentDepartmentId = dto.ParentDepartmentId;
            model.DepartmentType = dto.DepartmentType;
            model.ParentOrganizationId = dto.ParentOrganizationId;
            model.EducationProcessForm = dto.EducationProcessForm;
            model.EducationalDirections = dto.EducationalDirections ?? [];
            model.EducationLevelProvided = dto.EducationLevelProvided ?? [];
            model.OrganizationSpecialization = dto.OrganizationSpecialization ?? [];
            model.HasConsultationUnit = dto.HasConsultationUnit;
            model.GeneralSchedule = dto.GeneralSchedule;
            model.AdditionalDescription = dto.AdditionalDescription;
            model.OperationalStatus = dto.OperationalStatus;
            model.IsLocatedInMountains = dto.IsLocatedInMountains ?? throw new InvalidOperationException("IsLocatedInMountains is required.");
            model.IsBranchUnit = dto.IsBranchUnit;
            model.ZpoType = dto.ZpoType;
            
            if (dto.Contacts != null)
            {
                model.Contacts = dto.Contacts.ToModel();
            }
            
            return model;
        }
}
using OutOfSchool.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Models.ContactInfo;

namespace OutOfSchool.Services.Models;

public class Department : BusinessEntity, IHasContacts
{
    [MaxLength(Constants.MaxDescriptionLength)]
    public string Description { get; set; }
    
    [MaxLength(Constants.NameMaxLength)]
    public string ShortName { get; set; }
    
    [MaxLength(Constants.NameMaxLength)]
    public string GenitiveName { get; set; }
    
    [Required(ErrorMessage = "FullName is required.")]
    [MaxLength(Constants.NameMaxLength)]
    public string FullName { get; set; }

    public string Abbreviation { get; set; }
    
    public Guid? ParentDepartmentId { get; set; }
    
    [Required(ErrorMessage = "DepartmentType is required.")]
    public string DepartmentType { get; set; }
    
    #region AIKOM fields
    public Guid ParticipantId { get; set; }
    #endregion

    [Required(ErrorMessage = "ParentOrganizationId is required.")]
    public Guid ParentOrganizationId { get; set; }

    public string EducationProcessForm { get; set; }

    public List<string> EducationalDirections { get; set; } = [];

    public List<string> EducationLevelProvided { get; set; } = [];

    public List<string> OrganizationSpecialization { get; set; } = [];

    public bool HasConsultationUnit { get; set; }

    public string GeneralSchedule { get; set; }

    public string AdditionalDescription { get; set; }

    public string OperationalStatus { get; set; }

    [Required(ErrorMessage = "IsLocatedInMountains is required.")]
    public bool IsLocatedInMountains { get; set; }

    public bool IsBranchUnit { get; set; }

    public string ZpoType { get; set; }

    #region Owned entities

    public List<Contacts> Contacts { get; set; } = [];

    #endregion
}

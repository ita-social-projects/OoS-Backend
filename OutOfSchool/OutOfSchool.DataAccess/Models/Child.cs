using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Common;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class Child : IKeyedEntity<Guid>, ISoftDeleted
{
    public Guid Id { get; set; }

    public bool IsDeleted { get; set; }

    [MaxLength(Constants.NameMaxLength)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(Constants.NameMaxLength)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(Constants.NameMaxLength)]
    public string MiddleName { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    public string PlaceOfStudy { get; set; }

    // TODO: validate case when child has registered without parent help
    public Guid ParentId { get; set; }

    public bool IsParent { get; set; }

    public virtual Parent Parent { get; set; }

    public virtual List<SocialGroup> SocialGroups { get; set; }

    public virtual List<Achievement> Achievements { get; set; }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models;

public class Child : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [MaxLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(60)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(60)]
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
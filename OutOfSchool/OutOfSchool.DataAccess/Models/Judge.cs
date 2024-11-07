using System;
using System.ComponentModel.DataAnnotations;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.CompetitiveEvents;

namespace OutOfSchool.Services.Models;
public class Judge : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    [MaxLength(60)]
    public string FirstName { get; set; }

    [MaxLength(60)]
    public string LastName { get; set; }

    [MaxLength(60)]
    public string MiddleName { get; set; }

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Description { get; set; }

    public string CoverImageId { get; set; }

    public Guid CompetetiveEventId { get; set; }

    public virtual CompetitiveEvent CompetitiveEvent { get; set; }
}

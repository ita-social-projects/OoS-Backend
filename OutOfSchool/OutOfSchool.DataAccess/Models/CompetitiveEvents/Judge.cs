using System;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.Services.Models.CompetitiveEvents;
public class Judge : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public bool IsChiefJudge { get; set; }

    public string Description { get; set; }

    public string CoverImageId { get; set; }

    public Guid? CompetitiveEventId { get; set; }

    public virtual CompetitiveEvent CompetitiveEvent { get; set; }
}
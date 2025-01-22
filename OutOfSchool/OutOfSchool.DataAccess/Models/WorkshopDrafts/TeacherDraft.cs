using System;
using System.Collections.Generic;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models.WorkshopDrafts;

/// <summary>
/// Model for storing drafts before moderation.
/// Сan be hard deleted from the database if needed.
/// </summary>
public class TeacherDraft : 
    IImageDependentEntity<TeacherDraft>,
    IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public Gender Gender { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string Description { get; set; }

    public string CoverImageId { get; set; }

    public Guid WorkshopDraftId { get; set; }

    public bool IsDefaultTeacher { get; set; }

    public byte[] Version { get; set; }

    public virtual List<Image<TeacherDraft>> Images { get; set; }

    public virtual WorkshopDraft WorkshopDraft { get; set; }
}
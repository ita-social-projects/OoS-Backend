using System;

namespace OutOfSchool.Services.Models;
/// <summary>
/// Table for many-to-many relationship between StudySubject and Language
/// </summary>
public class StudySubjectLanguage : IKeyedEntity<long>
{
    public long Id {  get; set; }

    /// <summary>
    /// Id of the StudySubject entity.
    /// </summary>
    public Guid StudySubjectId { get; set; }
    public virtual StudySubject StudySubject { get; set; }

    /// <summary>
    /// Id of the Language entity.
    /// </summary>
    public long LanguageId { get; set; }
    public virtual Language Language { get; set; }
}

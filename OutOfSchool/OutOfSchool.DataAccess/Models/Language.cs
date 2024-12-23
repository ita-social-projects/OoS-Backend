using System.Collections.Generic;

namespace OutOfSchool.Services.Models;

/// <summary>
/// Represents a language used in the educational system.
/// </summary>
public class Language : IKeyedEntity<long>
{
    public long Id { get; set; }    
    public string Code { get; set; }
    public string Title { get; set; }

    public virtual List<EducationalDiscipline> EducationalDisciplines { get; set; }
}


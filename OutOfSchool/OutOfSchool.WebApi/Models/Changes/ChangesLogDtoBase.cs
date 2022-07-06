using System;

namespace OutOfSchool.WebApi.Models.Changes;

public abstract class ChangesLogDtoBase
{
    public string FieldName { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }

    public DateTime UpdatedDate { get; set; }

    public ShortUserDto User { get; set; }

    public string InstitutionTitle { get; set; }
}
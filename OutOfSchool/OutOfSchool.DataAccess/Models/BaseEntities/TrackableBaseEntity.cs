using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Services.Models.BaseEntities;
public abstract class TrackableBaseEntity
{
    public string CreatedBy { get; private set; }

    public string ModifiedBy { get; private set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset CreatedAt { get; private set; }

    [DataType(DataType.DateTime)]
    public DateTimeOffset ModifiedAt { get; private set; }
}

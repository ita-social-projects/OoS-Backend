using System;

namespace OutOfSchool.WebApi.Models.Changes;

public abstract class ChangesLogFilterBase : SearchStringFilter
{
    public string PropertyName { get; set; }

    public string EntityId { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}
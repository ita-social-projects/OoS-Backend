using System;
using System.Collections.Generic;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Enums.CompetitiveEvent;

namespace OutOfSchool.ElasticsearchData.Models;

public class CompetitiveEventFilterES
{
    public List<Guid> Ids { get; set; } = [];

    public string SearchText { get; set; } = string.Empty;

    public IReadOnlyCollection<CompetitiveEventStates> States { get; set; } = new List<CompetitiveEventStates>();

    public DateTimeOffset MinRegistrationEndTime { get; set; } = DateTimeOffset.MinValue;

    public DateTimeOffset MaxRegistrationEndTime { get; set; } = DateTimeOffset.MaxValue;

    public int Size { get; set; } = 12;

    public int From { get; set; } = 0;

    public DateTimeOffset MinScheduledStartTime { get; set; } = DateTimeOffset.MinValue;

    public DateTimeOffset MaxScheduledStartTime { get; set; } = DateTimeOffset.MaxValue;

    public IReadOnlyCollection<FormOfLearning> PlannedFormatsOfClasses { get; set; } = new List<FormOfLearning>();

    public bool AreThereBenefits { get; set; }

    public int MinimumAge { get; set; } = 0;

    public int MaximumAge { get; set; } = 100;

    public int MinPrice { get; set; } = 0;

    public int MaxPrice { get; set; } = int.MaxValue;

    public bool CompetitiveSelection { get; set; }

    public bool IsPaid { get; set; }
}

using System;
using System.Collections.Generic;
using OutOfSchool.Common.Enums;
using OutOfSchool.ElasticsearchData.Enums;

namespace OutOfSchool.ElasticsearchData.Models;

public class WorkshopFilterES
{
    public List<Guid> Ids { get; set; } = null;

    public string SearchText { get; set; } = string.Empty;

    public string OrderByField { get; set; } = OrderBy.Id.ToString();

    public int MinAge { get; set; } = 0;

    public int MaxAge { get; set; } = 100;

    public bool IsFree { get; set; } = false;

    public int MinPrice { get; set; } = 0;

    public int MaxPrice { get; set; } = int.MaxValue;

    public List<long> DirectionIds { get; set; } = new List<long>();

    public string City { get; set; } = string.Empty;

    public bool WithDisabilityOptions { get; set; } = false;

    public string Workdays { get; set; } = string.Empty;

    public TimeSpan MinStartTime { get; set; } = new TimeSpan(0, 0, 0);

    public TimeSpan MaxStartTime { get; set; } = new TimeSpan(23, 59, 59);

    public int Size { get; set; } = 12;

    public int From { get; set; } = 0;

    public decimal Latitude { get; set; } = 0;

    public decimal Longitude { get; set; } = 0;

    public IReadOnlyCollection<WorkshopStatus> Statuses { get; set; } = new List<WorkshopStatus>();

    public bool IsAppropriateAge { get; set; } = false;
    public bool IsAppropriateHours { get; set; } = false;

    public bool IsStrictWorkdays { get; set; } = false;
}
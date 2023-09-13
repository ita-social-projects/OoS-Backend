using System;

namespace OutOfSchool.Services.Models;

public class QuartzJob : IKeyedEntity<long>, ISoftDeleted
{
    public long Id { get; set; }

    public bool IsDeleted { get; set; }

    public string Name { get; set; }

    public DateTimeOffset LastSuccessLaunch { get; set; }
}

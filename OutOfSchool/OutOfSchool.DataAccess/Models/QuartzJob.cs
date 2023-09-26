using System;

namespace OutOfSchool.Services.Models;

public class QuartzJob : IKeyedEntity<long>
{
    public long Id { get; set; }

    public string Name { get; set; }

    public DateTimeOffset LastSuccessLaunch { get; set; }
}

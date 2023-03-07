using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.Services.Models;
public class QuartzJob : IKeyedEntity<long>
{
    public long Id { get; set; }

    public string Name { get; set; }

    public DateTimeOffset LastSuccessLaunch { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutOfSchool.AuthCommon.Config;
public class ChangesLogConfig
{
    public const string Name = "ChangesLog";

    public IReadOnlyDictionary<string, string[]> TrackedProperties { get; set; }
}

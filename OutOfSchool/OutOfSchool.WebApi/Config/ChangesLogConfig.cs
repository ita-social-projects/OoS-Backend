using System.Collections.Generic;

namespace OutOfSchool.WebApi.Config
{
    public class ChangesLogConfig
    {
        public const string Name = "ChangesLog";

        public IReadOnlyDictionary<string, string[]> TrackedFields { get; set; }
    }
}

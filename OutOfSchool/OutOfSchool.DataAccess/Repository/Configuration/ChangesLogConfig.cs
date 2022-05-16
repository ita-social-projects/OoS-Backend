using System.Collections.Generic;

namespace OutOfSchool.Services.Repository.Configuration
{
    public class ChangesLogConfig
    {
        public const string Name = "ChangesLog";

        public Dictionary<string, string[]> SupportedFields { get; set; }
    }
}

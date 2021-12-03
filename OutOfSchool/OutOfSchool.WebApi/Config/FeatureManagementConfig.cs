using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Config
{
    public class FeatureManagementConfig
    {
        public const string Name = "FeatureManagement";

        public bool Release1 { get; set; }

        public bool Release2 { get; set; }

        public bool Release3 { get; set; }
    }
}

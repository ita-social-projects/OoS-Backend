using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Config
{
    public class RequestLimitsOptions
    {
        public const string Name = "RequestLimits";

        public byte MaxCountOfAttachedFiles { get; set; }
    }
}

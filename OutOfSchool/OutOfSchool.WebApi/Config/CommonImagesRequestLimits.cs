using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Config
{
    /// <summary>
    /// Describes common limits of requests with images.
    /// </summary>
    public class CommonImagesRequestLimits
    {
        public const string Name = "CommonImagesRequestLimits";

        public byte MaxCountOfAttachedFiles { get; set; }
    }
}

using System.Collections.Generic;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Images
{
    public class MultipleImageRemovingResponse
    {
        public bool AllImagesRemoved { get; set; }

        public string GeneralMessage { get; set; }

        public bool HasResults { get; set; }

        public IDictionary<short, OperationResult> Results { get; set; }
    }
}

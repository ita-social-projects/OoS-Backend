using System.Collections.Generic;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Images
{
    public class ImageRemovingResult
    {
        public List<string> RemovedIds { get; set; }

        public MultipleKeyValueOperationResult MultipleKeyValueOperationResult { get; set; }
    }
}

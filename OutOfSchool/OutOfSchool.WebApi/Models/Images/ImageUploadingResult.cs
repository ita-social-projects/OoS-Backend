using System.Collections.Generic;
using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Images
{
    public class ImageUploadingResult
    {
        public List<string> SavedIds { get; set; }

        public MultipleKeyValueOperationResult MultipleKeyValueOperationResult { get; set; }
    }
}

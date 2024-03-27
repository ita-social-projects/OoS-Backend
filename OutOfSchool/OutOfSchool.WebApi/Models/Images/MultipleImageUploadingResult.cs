using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models.Images;

public class MultipleImageUploadingResult
{
    public List<string> SavedIds { get; set; }

    public MultipleKeyValueOperationResult MultipleKeyValueOperationResult { get; set; }
}
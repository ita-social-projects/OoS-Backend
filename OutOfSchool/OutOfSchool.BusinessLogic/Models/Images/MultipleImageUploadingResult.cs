using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Models.Images;

public class MultipleImageUploadingResult
{
    public List<string> SavedIds { get; set; }

    public MultipleKeyValueOperationResult MultipleKeyValueOperationResult { get; set; }
}
using OutOfSchool.BusinessLogic.Common;

namespace OutOfSchool.BusinessLogic.Models.Images;

public class MultipleImageRemovingResult
{
    public List<string> RemovedIds { get; set; }

    public MultipleKeyValueOperationResult MultipleKeyValueOperationResult { get; set; }
}
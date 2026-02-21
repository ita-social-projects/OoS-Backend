namespace OutOfSchool.BusinessLogic.Models;

public class ChildrenCreationResultDto
{
    public ParentDTO Parent { get; set; }

    public List<ChildCreationResult> ChildrenCreationResults { get; set; } = new List<ChildCreationResult>();
}

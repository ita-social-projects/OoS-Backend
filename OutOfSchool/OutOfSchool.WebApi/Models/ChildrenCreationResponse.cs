namespace OutOfSchool.WebApi.Models;

public class ChildrenCreationResponse
{
    public ParentDTO Parent { get; set; }

    public List<ChildCreationResult> ChildrenCreationResults { get; set; }
}

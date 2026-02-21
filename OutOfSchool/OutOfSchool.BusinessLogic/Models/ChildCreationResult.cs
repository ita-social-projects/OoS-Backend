namespace OutOfSchool.BusinessLogic.Models;

public class ChildCreationResult
{
    public ChildBaseDto Child { get; set; }

    public bool IsSuccess { get; set; }

    public string Message { get; set; }
}
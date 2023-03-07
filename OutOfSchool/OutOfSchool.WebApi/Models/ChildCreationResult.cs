using OutOfSchool.WebApi.Common;

namespace OutOfSchool.WebApi.Models;

public class ChildCreationResult
{
    public ChildDto Child { get; set; }

    public bool IsSuccess { get; set; }

    public string Message { get; set; }
}
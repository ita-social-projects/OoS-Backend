namespace OutOfSchool.AuthCommon.Models;

public class IdGovErrorResponse
{
    public int Error { get; set; } = default;

    public string? Message { get; set; } = null;
}
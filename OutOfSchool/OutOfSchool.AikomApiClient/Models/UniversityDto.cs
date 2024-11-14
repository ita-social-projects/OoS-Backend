namespace OutOfSchool.AikomApiClient.Models;

public class UniversityDto
{
    public int Id { get; set; }
    
    public required string UniversityFullName { get; set; }

    public bool? IsBranch { get; set; } = null;

    public required string Edrpou { get; set; }
    
    public IEnumerable<BranchDto>? Branches { get; set; }
}

public class BranchDto
{
    public int BranchId { get; set; }
    public required string BranchName { get; set; }
    public string? Edrpou { get; set; }
}

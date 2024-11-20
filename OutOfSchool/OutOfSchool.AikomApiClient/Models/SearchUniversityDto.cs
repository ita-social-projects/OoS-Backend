namespace OutOfSchool.AikomApiClient.Models;

public class SearchUniversityDto
{
    public required int Id { get; set; }
    
    public required string UniversityFullName { get; set; }

    public bool? IsBranch { get; set; } = null;

    public required string Edrpou { get; set; }
    
    public IEnumerable<BranchDto>? Branches { get; set; }
}

public class BranchDto
{
    public required int BranchId { get; set; }

    public required string BranchName { get; set; }
    
    public string? Edrpou { get; set; }
}

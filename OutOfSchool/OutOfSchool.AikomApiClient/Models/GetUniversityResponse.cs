namespace OutOfSchool.AikomApiClient.Models;

public class GetUniversityResponse : ApiResponse<GetUniversityResponseData>
{
}

public class GetUniversityResponseData
{
    public required string FullName { get; set; }

    public required string ShortName { get; set; }

    public required string Address { get; set; }

    public required string Email { get; set; }

    public required string Phone { get; set; }

    public required UniversityBoss UniversityBoss { get; set; }
}

public class UniversityBoss
{
    public required string BossLastName { get; set; }

    public required string BossFirstName { get; set; }

    public string? BossMiddleName { get; set; }

    public string? BossRnokpp { get; set; }

    public required string UniversityBossEMail { get; set; }
}


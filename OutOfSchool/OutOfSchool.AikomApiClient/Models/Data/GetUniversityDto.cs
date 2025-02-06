namespace OutOfSchool.AikomApiClient.Models.Data;

public class GetUniversityDto
{
    public required string FullName { get; set; }

    public required string ShortName { get; set; }

    public required string Address { get; set; }

    public required string Email { get; set; }

    public required string Phone { get; set; }

    public required UniversityBossDto UniversityBoss { get; set; }
}

public class UniversityBossDto
{
    public required string BossLastName { get; set; }

    public required string BossFirstName { get; set; }

    public string? BossMiddleName { get; set; }

    public string? BossRnokp { get; set; }

    public required string UniversityBossEMail { get; set; }
}
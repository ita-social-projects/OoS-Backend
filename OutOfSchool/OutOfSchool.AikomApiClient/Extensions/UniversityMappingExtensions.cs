using OutOfSchool.AikomApiClient.Models.Data;
using OutOfSchool.AikomApiClient.Models.Responses;

namespace OutOfSchool.AikomApiClient.Extensions;

public static class UniversityMappingExtensions
{
    public static SearchUniversityDto ToDto(this SearchUniversityResponseData data) => new()
    {
        Id = data.Id,
        UniversityFullName = data.UniversityFullName,
        IsBranch = data.IsBranch,
        Edrpou = data.Edrpou,
        Branches = data.Branches?.Select(b => b.ToDto()).ToList() ?? []
    };

    public static BranchDto ToDto(this Branch branch) => new()
    {
        BranchId = branch.BranchId,
        BranchName = branch.BranchName,
        Edrpou = branch.Edrpou,
    };

    public static GetUniversityDto ToDto(this GetUniversityResponseData data) => new()
    {
        FullName = data.FullName,
        ShortName = data.ShortName,
        Address = data.Address,
        Email = data.Email,
        Phone = data.Phone,
        UniversityBoss = data.UniversityBoss.ToDto()
    };

    public static UniversityBossDto ToDto(this UniversityBoss boss) => new()
    {
        BossLastName = boss.BossLastName,
        BossFirstName = boss.BossFirstName,
        BossMiddleName = boss.BossMiddleName,
        BossRnokp = boss.BossRnokpp,
        UniversityBossEMail = boss.UniversityBossEMail
    };
}

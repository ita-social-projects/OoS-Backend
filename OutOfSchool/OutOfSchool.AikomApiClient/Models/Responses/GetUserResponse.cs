namespace OutOfSchool.AikomApiClient.Models.Responses;

internal class GetUserResponseData
{
    public required string UserCommonId { get; set; }

    public required string UserStatus { get; set; }

    public required string OrganizationId { get; set; }

    public string? UserRole { get; set; }
}

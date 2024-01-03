namespace OutOfSchool.WebApi.Config;

public class CacheProfilesConfig
{
    public const string Name = "CacheProfiles";

    public int PrivateDurationInSeconds { get; set; }

    public int PublicDurationInSeconds { get; set; }
}

namespace OutOfSchool.Common.Config;

public interface IMySqlGuidConnectionOptions : IMySqlConnectionOptions
{
    public bool OldGuids { get; set; }
}
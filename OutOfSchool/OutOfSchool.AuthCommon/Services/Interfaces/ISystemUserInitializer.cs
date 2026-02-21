namespace OutOfSchool.AuthCommon.Services.Interfaces;
public interface ISystemUserInitializer
{
    /// <summary>
    /// Ensures that the system user exists in the identity store.
    /// </summary>
    public Task EnsureExistsAsync(CancellationToken cancellationToken = default);
}

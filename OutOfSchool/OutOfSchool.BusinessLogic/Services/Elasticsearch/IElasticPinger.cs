namespace OutOfSchool.BusinessLogic.Services;

public interface IElasticPinger
{
    bool IsHealthy { get; }
}

namespace OutOfSchool.BusinessLogic.Services;

public interface IElasticsearchHealthService
{
    bool IsHealthy { get; }
}

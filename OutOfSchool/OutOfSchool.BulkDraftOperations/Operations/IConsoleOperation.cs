using Microsoft.Extensions.Hosting;

namespace OutOfSchool.BulkDraftOperations.Operations;

public interface IConsoleOperation
{
    string Name { get; }
    string Description { get; }

    void ConfigureHost(IHostBuilder hostBuilder, string[] args);

    Task<int> RunAsync(IHost host, string[] args, CancellationToken cancellationToken);
}



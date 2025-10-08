using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OutOfSchool.BulkDraftOperations.Operations;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;

var (operationName, operationArgs) = ParseArgs(args);

var availableOperations = new IConsoleOperation[]
{
    new ApproveWorkshopDraftsOperation(),
    new ConvertWorkshopsToDraftsOperation(),
};

if (string.IsNullOrWhiteSpace(operationName) || IsHelp(operationName))
{
    PrintHelp(availableOperations);
    return 0;
}

var selectedOperation = availableOperations
    .FirstOrDefault(o => string.Equals(o.Name, operationName, StringComparison.OrdinalIgnoreCase));

if (selectedOperation is null)
{
    Console.WriteLine($"Unknown operation: {operationName}");
    PrintHelp(availableOperations);
    return 1;
}

var builder = Host.CreateDefaultBuilder(operationArgs)
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(operationArgs);
    })
    .UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
            .WithDefaultDestructurers()
            .WithDestructurers([new DbUpdateExceptionDestructurer()])));

selectedOperation.ConfigureHost(builder, operationArgs);

using var host = builder.Build();

try
{
    var exitCode = await selectedOperation.RunAsync(host, operationArgs, CancellationToken.None);
    return exitCode;
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error: {ex.Message}");
    throw;
}

static (string operation, string[] rest) ParseArgs(string[] args)
{
    return args.Length == 0 ? (string.Empty, []) : (args[0], args.Skip(1).ToArray());
}

static bool IsHelp(string value)
{
    return string.Equals(value, "help", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "/?", StringComparison.OrdinalIgnoreCase);
}

static void PrintHelp(IReadOnlyCollection<IConsoleOperation> operations)
{
    Console.WriteLine("Usage: dotnet run -- <operation> [args]\n");
    Console.WriteLine("Available operations:");
    foreach (var op in operations.OrderBy(o => o.Name))
    {
        Console.WriteLine($"  {op.Name,-12} {op.Description}");
    }
    Console.WriteLine("\nExamples:");
    Console.WriteLine("  dotnet run -- approve --file=workshops.json");
    Console.WriteLine("  dotnet run -- convert --since=2025-09-10");
}

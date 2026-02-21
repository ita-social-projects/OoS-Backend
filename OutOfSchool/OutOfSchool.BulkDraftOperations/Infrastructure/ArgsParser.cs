namespace OutOfSchool.BulkDraftOperations.Operations;

public static class ArgsParser
{
    public static string? GetArgValue(string[] args, string key)
    {
        if (args.Length == 0)
        {
            return null;
        }

        var prefix = $"--{key}=";
        var match = args.FirstOrDefault(a => a.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        if (match is not null)
        {
            return match[prefix.Length..];
        }

        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], $"--{key}", StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return null;
    }
}
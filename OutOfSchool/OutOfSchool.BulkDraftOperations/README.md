# OutOfSchool Bulk Draft Operations

This application provides command-line tools for managing workshop drafts, including converting existing workshops to drafts and approving workshop drafts in bulk.

## Available Operations

### 1. Approve Workshop Drafts (`approve`)

Approves workshop drafts from a JSON file containing workshop IDs.

Usage:
```bash
dotnet run -- approve --file=workshops.json
```

Parameters:
- `--file` or `-f`: Path to JSON file containing workshop IDs (default: `workshops_approved.json`)

JSON Format:
```json
[
  { "id": "workshop-guid-1" },
  { "id": "workshop-guid-2" }
]
```

### 2. Convert Workshops to Drafts (`convert`)

Converts workshops created since a specified date into draft status.

Usage:
```bash
dotnet run -- convert --since=2025-01-10
```

Parameters:
- `--since`, `--date`, or `--after`: Date in YYYY-MM-DD format (default: 6 hours ago)

## Getting Help

To see available operations and usage examples:

```bash
dotnet run -- help
```

## Configuration

The application uses standard .NET configuration sources:
- `appsettings.json`
- Environment-specific settings files
- User secrets
- Environment variables
- Command line arguments

## Examples

```bash
# Approve workshops from a specific file
dotnet run -- approve --file=my-workshops.json

# Convert workshops created since a specific date
dotnet run -- convert --since=2025-01-01

# Show help
dotnet run -- help
```

## Exit Codes

- `0`: Success
- `1`: General error (file not found, invalid arguments, etc.)
- `2`: Partial success (some operations failed during approval)

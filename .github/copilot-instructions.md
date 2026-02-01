# SendHub Copilot Instructions

## Architecture Overview

SendHub is a file monitoring and distribution system built with .NET 10, using a clean architecture with three main layers:

- **SendHub.Daemon**: Main entry point - Windows Service that watches folders using `FolderWatcher` BackgroundService
- **SendHub.Features**: Business logic layer implementing command/handler pattern with `ICommand`/`ICommandHandler<T>`
- **SendHub.Infrastructure**: External integrations (file system, email, messaging providers)
- **SendHub**: Core abstractions and interfaces

## Key Patterns & Conventions

### Command/Handler Pattern
All business operations use commands:
```csharp
public record ProcessIncomingFile(FileInfo File) : ICommand;
public class ProcessIncomingFileHandler : ICommandHandler<ProcessIncomingFile>
```

### Dependency Injection Registration
Each layer has a `Registration.cs` with extension methods:
```csharp
services.AddSendHubFeatures()  // Features layer
services.AddSendHubInfrastructure()  // Infrastructure layer
```

### Testing Strategy
- Uses `System.IO.Abstractions.TestingHelpers.MockFileSystem` for file system mocking
- Tests are organized by class in subfolders: `FolderWatcherTests/StartTests.cs`
- Moq for mocking dependencies
- Projects expose internals via `<InternalsVisibleTo>` for testing

### Configuration
- Uses strongly-typed settings classes bound via `IOptions<T>`
- Environment variable prefix: `SendHub_`
- Hierarchical config binding: `SendHub:Email:Smtp:Host`

## Development Workflows

### Build & Run
```bash
dotnet build              # Build solution
dotnet test              # Run all tests  
dotnet run --project src/SendHub.Daemon  # Run daemon
```

### Aspire Integration
- Use `src/Aspire/SendHub.AppHost` for local development orchestration
- Aspire manages the daemon project as a distributed application component

### Package Management
- Centralized via `Directory.Packages.props` with `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`
- All `PackageReference` elements in project files omit Version attribute
- **Critical**: Watch for duplicate `PackageVersion` entries (causes NU1506 errors)

## File Organization Rules

### Project Structure
- Daemon, Features, Infrastructure follow screaming architecture (feature folders)
- Value objects in dedicated `ValueObjects/` folder  
- Log messages in separate `*LogMessages.cs` files using source generators
- Fakes/test doubles in `Fakes/` folder within test projects

### Naming Conventions
- Interfaces: `ICommandHandler<T>`, `IFileScanner`
- Settings classes: `*Settings` (e.g., `FolderWatcherSettings`)
- Test classes: `{ClassUnderTest}Tests/{MethodUnderTest}Tests.cs`
- Commands: Verb + noun (e.g., `ProcessIncomingFile`)

## Integration Points

### File System
- Always use `System.IO.Abstractions.IFileSystem` for testability
- File watching via custom `IFileSystemWatcher` abstraction over `System.IO.FileSystemWatcher`
- Channel-based queuing for file processing: `Channel.CreateUnbounded<FileInfo>()`

### Configuration Sources (in order)
1. `appsettings.json`
2. `appsettings.{Environment}.json`  
3. User secrets
4. Environment variables (with `SendHub_` prefix)

### Logging
- Serilog with structured logging
- Enrichers: MachineName, ProcessId, ThreadId, UtcTime
- Log messages in dedicated classes using source generators
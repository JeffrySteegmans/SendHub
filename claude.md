# SendHub Project

## Overview

**SendHub** is a .NET automation tool that monitors a folder for new files and automatically sends them as email attachments to configured recipients. It streamlines file distribution workflows with minimal configuration, making it ideal for:

- Automating document workflows
- Reports distribution
- File sharing processes
- Continuous monitoring of scan folders

**Current Status:** MVP complete — folder monitoring, email delivery, file archiving, and idempotency tracking are all fully implemented and tested.

## Architecture

SendHub follows **Clean Architecture** principles with clear separation of concerns:

**Pattern:** Clean Architecture + Command Pattern + Dependency Injection

**Main Workflow:**

```text
FolderWatcher (BackgroundService)
├── Startup: Creates watchers and spawns 3 worker threads
├── File Discovery: Scans existing files on startup
├── Real-time Monitoring: FileSystemWatcher for new files
├── Processing Queue: Channel-based async queue
└── Command Handlers: Process files via ICommandHandler<T>
```

**Key Design Principles:**

- Interface-based abstractions for testability
- System.IO.Abstractions for file system mocking
- Structured logging with Serilog
- Async/await throughout for performance
- Proper cancellation token support

## Solution Structure

### Core Projects

#### SendHub (Library)

- Base abstractions and interfaces
- Core types: `ICommand`, `ICommandHandler<T>`, `IFileScanner`, `IFileSender`, `IFileSystemWatcher`, `IFileSystemWatcherFactory`, `IFileArchiver`, `IProcessedFileTracker`
- Value Objects: `DirectoryPath` (validated directory paths)

#### SendHub.Daemon (Worker Service)

- Main executable application
- [FolderWatcher.cs](src/SendHub.Daemon/FolderWatcher.cs): Orchestrates file monitoring workflow
- [FolderWatcherSettings.cs](src/SendHub.Daemon/FolderWatcherSettings.cs): Configuration model
- [Program.cs](src/SendHub.Daemon/Program.cs): Entry point with DI setup

#### SendHub.Features (Library)

- Business logic layer using command pattern
- [FileProcessing/](src/SendHub.Features/FileProcessing/): File processing commands and handlers
  - `ProcessIncomingFile`: Command record (file + destination folder)
  - `ProcessIncomingFileHandler`: 2-phase processing — send to all senders in parallel, then archive and mark as processed only if all succeed
  - `FileSendException`: Aggregates per-sender failures for error reporting

#### SendHub.Infrastructure (Library)

- FileSystem: File scanning, watching, and archiving implementations
- Messaging: Email sending via SMTP (`SmtpFileSender`)
- Database: SQLite database settings (`DatabaseSettings`)
- Tracking: SQLite-based processed file tracking (`SqliteFileTracker`) for idempotency with automatic JSON migration

### Test Projects

#### SendHub.Daemon.Tests

- Unit tests for FolderWatcher background service
- Uses Moq and System.IO.Abstractions.TestingHelpers

#### SendHub.Features.Tests

- Unit tests for features layer
- `ProcessIncomingFileHandlerTests`: pending implementation

#### SendHub.Infrastructure.Tests

- Unit tests for infrastructure layer
- Covers `FileArchiver` (move, conflict resolution), `SmtpFileSender` (settings validation), and `SqliteFileTracker` (persistence, idempotency, schema creation, legacy JSON migration)

### Aspire Projects

**SendHub.AppHost** - .NET Aspire orchestration host

**SendHub.ServiceDefaults** - Shared service configuration

## Key Components

### 1. FolderWatcher (Background Service)

Location: [src/SendHub.Daemon/FolderWatcher.cs](src/SendHub.Daemon/FolderWatcher.cs)

#### Responsibilities

- Monitors configured folder for new files
- Manages 3 concurrent worker threads
- Handles file queuing via System.Threading.Channels
- Auto-recovery on watcher errors
- Creates destination folder if missing

### 2. File System Abstractions

Location: [src/SendHub.Infrastructure/FileSystem/](src/SendHub.Infrastructure/FileSystem/)

#### Components

- `FileSystemScanner`: Scans directories for files
- `FileSystemWatcherAdapter`: Wraps System.IO.FileSystemWatcher
- `FileSystemWatcherFactory`: Factory for creating watchers
- `FileArchiver`: Archives processed files to the destination folder (moves file, handles name conflicts with counter suffix)

### 3. Command Pattern Implementation

Location: [src/SendHub/](src/SendHub/)

#### Implementation Details

- `ICommand`: Marker interface for commands
- `ICommandHandler<TCommand>`: Handler interface
- `ProcessIncomingFile`: Command for file processing

### 4. Processed File Tracking

Location: [src/SendHub.Infrastructure/Tracking/](src/SendHub.Infrastructure/Tracking/)

Ensures idempotency — files already processed are not sent again after restarts.

- `SqliteFileTracker`: Implements `IProcessedFileTracker`, persists state to a SQLite database
- `TrackedFile`: Value object holding file path and processed timestamp (used for JSON migration)
- Database schema:
  - `processed_files` table: Stores file paths and processed timestamps
  - `schema_version` table: Tracks database schema version for future migrations
- Automatic migration: Legacy JSON tracking files are automatically migrated to SQLite on first run

### 5. Configuration System

Location: [src/SendHub.Daemon/FolderWatcherSettings.cs](src/SendHub.Daemon/FolderWatcherSettings.cs)

#### Settings

`FolderWatcherSettings` (section: `SendHub`):

- `WatchFolder`: Folder to monitor
- `DestinationFolder`: Where processed files go

Email SMTP settings are configured separately under `SendHub:Email:Smtp` (host, port, username, password, SSL, from, to).

`DatabaseSettings` (section: `SendHub:Database`):

- `Path`: Path to the SQLite database file (default: `D:\SendHub\sendhub.db`)

## Technologies Used

### Runtime

- .NET 10.0 (latest LTS)
- C# 12+ with nullable reference types

### Core Frameworks

- Microsoft.Extensions.Hosting (Worker service host)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- System.IO.Abstractions (File system testability)

### Database

- Microsoft.Data.Sqlite (SQLite ADO.NET provider)
- Dapper (Micro-ORM for clean SQL queries)
- SQLite (embedded database for file tracking and future settings storage)

### Logging & Observability

- Serilog with structured logging
- Serilog enrichers (machine name, process ID, thread ID, UTC time)
- OpenTelemetry (instrumentation ready)

### Test Frameworks

- xUnit (test framework)
- Moq (mocking library)
- System.IO.Abstractions.TestingHelpers
- coverlet (code coverage)

### Orchestration

- .NET Aspire (distributed application runtime)

## Development Setup

### Prerequisites

- .NET 10.0 SDK or later
- IDE: Visual Studio 2022, VS Code, or JetBrains Rider

### Initial Setup

1. Clone the repository

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Configure user secrets (for development):

   ```bash
   cd src/SendHub.Daemon
   dotnet user-secrets set "SendHub:WatchFolder" "D:\ScanFolder"
   dotnet user-secrets set "SendHub:Email:Smtp:Password" "your-app-password"
   ```

4. Or edit [appsettings.json](src/SendHub.Daemon/appsettings.json):

   ```json
   {
     "SendHub": {
       "WatchFolder": "D:\\ScanFolder",
       "DestinationFolder": "D:\\ScanFolder\\Processed",
       "Database": {
         "Path": "D:\\SendHub\\sendhub.db"
       },
       "Email": {
         "Smtp": {
           "Host": "smtp.gmail.com",
           "Port": 587,
           "Username": "your-email@gmail.com",
           "Password": "your-app-password",
           "EnableSsl": true,
           "From": "sendhub@example.com",
           "To": "recipient@example.com"
         }
       }
     }
   }
   ```

### Configuration Methods

**Priority order (highest to lowest):**

1. Environment Variables (prefix: `SendHub_`, example: `SendHub_WatchFolder`)
2. User Secrets (development only)
3. appsettings.json

**User Secrets ID:** `17df84b2-8577-4b61-b466-e04c093bb95f`

## Building and Running

### Build the solution

```bash
dotnet build
```

### Run the daemon

```bash
cd src/SendHub.Daemon
dotnet run
```

### Run with Aspire orchestration

```bash
cd src/SendHub.AppHost
dotnet run
```

### Publish for deployment

```bash
dotnet publish src/SendHub.Daemon -c Release -o ./publish
```

## Testing

### Run all tests

```bash
dotnet test
```

### Run with coverage

```bash
dotnet test /p:CollectCoverage=true
```

### Test structure

- **Unit Tests:** FolderWatcher (daemon), FileArchiver, SmtpFileSender, JsonFileTracker (infrastructure)
- **Mocking:** Uses Moq for dependencies
- **File System:** System.IO.Abstractions.TestingHelpers for file system mocking
- **Time:** NodaTime.Testing FakeClock for deterministic timestamp tests

## Future Roadmap

Planned features for post-MVP:

- Web-based configuration interface
- Microsoft Teams integration
- Slack integration
- Webhook support
- Multiple recipient support

## Project Structure

```text
SendHub/
├── src/
│   ├── SendHub/                         # Core abstractions
│   ├── SendHub.Daemon/                  # Worker service (main executable)
│   ├── SendHub.Daemon.Tests/            # Unit tests for daemon
│   ├── SendHub.Features/                # Business logic
│   ├── SendHub.Features.Tests/          # Unit tests for features
│   ├── SendHub.Infrastructure/          # Cross-cutting concerns
│   ├── SendHub.Infrastructure.Tests/    # Unit tests for infrastructure
│   └── Aspire/
│       ├── SendHub.AppHost/             # Aspire orchestration
│       └── SendHub.ServiceDefaults/     # Shared service configuration
├── global.json                          # .NET SDK version pinning
├── Directory.Build.props                # Shared MSBuild properties
├── Directory.Packages.props             # Centralized NuGet package versions
└── SendHub.slnx                         # Solution file
```

## Blazor Conventions

### Code-Behind Files

**NEVER place C# code inside a `.razor` file.** All logic must live in a separate code-behind file:

- Every Razor component has a paired `ComponentName.razor.cs` file
- The code-behind class is `partial` and matches the component name exactly
- Use `[Inject]` attribute in the code-behind instead of `@inject` directives in the razor file
- Use `[Inject]` with `= default!` to suppress nullable warnings: `[Inject] private IFoo Foo { get; set; } = default!;`
- The `@inherits` directive stays in the razor file if needed (it is markup, not code)
- The razor file contains only markup, directives (`@page`, `@inherits`), and component references

**Example:**

```
Settings.razor       ← markup only (@page, HTML, MudBlazor components)
Settings.razor.cs    ← partial class with [Inject] properties and methods
```

## Additional Notes

### Concurrency Model

- Uses System.Threading.Channels for async queue between watcher and processors
- 3 concurrent worker threads for file processing
- Thread-safe file queuing and processing

### Logging

- Structured logging with contextual properties (machine name, process ID, thread ID)
- LoggerMessage source generators for performance
- Comprehensive logging for monitoring and debugging

### Error Handling

- Try-catch in file queuing
- Worker exception handling with logging
- FileSystemWatcher error recovery mechanism

### Value Objects

- `DirectoryPath`: Ensures validated directory paths throughout the application
- Prevents invalid paths from entering the system

### Branch Strategy

- Main branch: `master`
- Feature branch: `features/mvp` (current)

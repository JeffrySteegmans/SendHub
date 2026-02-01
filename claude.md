# SendHub Project

## Overview

**SendHub** is a .NET automation tool that monitors a folder for new files and automatically sends them as email attachments to configured recipients. It streamlines file distribution workflows with minimal configuration, making it ideal for:

- Automating document workflows
- Reports distribution
- File sharing processes
- Continuous monitoring of scan folders

**Current Status:** MVP phase with folder monitoring and email delivery as core features.

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
- Core types: `ICommand`, `ICommandHandler<T>`, `IFileScanner`, `IFileSender`, `IFileSystemWatcher`
- Value Objects: `DirectoryPath` (validated directory paths)

#### SendHub.Daemon (Worker Service)

- Main executable application
- [FolderWatcher.cs](src/SendHub.Daemon/FolderWatcher.cs): Orchestrates file monitoring workflow
- [FolderWatcherSettings.cs](src/SendHub.Daemon/FolderWatcherSettings.cs): Configuration model
- [Program.cs](src/SendHub.Daemon/Program.cs): Entry point with DI setup

#### SendHub.Features (Library)

- Business logic layer using command pattern
- [FileProcessing/](src/SendHub.Features/FileProcessing/): File processing commands and handlers

#### SendHub.Infrastructure (Library)

- FileSystem: File scanning and watching implementations
- Messaging: Abstraction for sending files (Email, Teams, etc.)
- Tracking: Future logging/tracking functionality

### Test Projects

#### SendHub.Daemon.Tests

- Unit tests for FolderWatcher background service
- Uses Moq and System.IO.Abstractions.TestingHelpers

#### SendHub.Features.Tests

- Unit tests for features layer

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

### 3. Command Pattern Implementation

Location: [src/SendHub/](src/SendHub/)

#### Implementation Details

- `ICommand`: Marker interface for commands
- `ICommandHandler<TCommand>`: Handler interface
- `ProcessIncomingFile`: Command for file processing

### 4. Configuration System

Location: [src/SendHub.Daemon/FolderWatcherSettings.cs](src/SendHub.Daemon/FolderWatcherSettings.cs)

#### Settings

- WatchFolder: Folder to monitor
- DestinationFolder: Where processed files go
- Email configuration (SMTP settings)

## Technologies Used

### Runtime

- .NET 10.0 (latest LTS)
- C# 12+ with nullable reference types

### Core Frameworks

- Microsoft.Extensions.Hosting (Worker service host)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- System.IO.Abstractions (File system testability)

### Logging & Observability

- Serilog with structured logging
- Serilog enrichers (machine name, process ID, thread ID, UTC time)
- OpenTelemetry (instrumentation ready)

### Testing

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

- **Unit Tests:** FolderWatcher, file processing handlers
- **Mocking:** Uses Moq for dependencies
- **File System:** System.IO.Abstractions.TestingHelpers for file system mocking

## Future Roadmap

Planned features for post-MVP:

- Web-based configuration interface
- Activity logging and history
- Microsoft Teams integration
- Slack integration
- Webhook support
- Multiple recipient support

## Project Structure

```text
SendHub/
├── src/
│   ├── SendHub/                    # Core abstractions
│   ├── SendHub.Daemon/             # Worker service (main executable)
│   ├── SendHub.Features/           # Business logic
│   ├── SendHub.Infrastructure/     # Cross-cutting concerns
│   ├── SendHub.AppHost/           # Aspire orchestration
│   └── SendHub.ServiceDefaults/   # Shared configuration
├── tests/
│   ├── SendHub.Daemon.Tests/
│   └── SendHub.Features.Tests/
├── global.json                     # .NET SDK version
├── Directory.Build.props           # Shared MSBuild properties
└── SendHub.slnx                   # Solution file
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

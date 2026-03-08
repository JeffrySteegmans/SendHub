# SendHub

[![Release Docker Image](https://github.com/JeffrySteegmans/SendHub/actions/workflows/release.yml/badge.svg)](https://github.com/JeffrySteegmans/SendHub/actions/workflows/release.yml)
[![GitHub Release](https://img.shields.io/github/v/release/JeffrySteegmans/SendHub)](https://github.com/JeffrySteegmans/SendHub/releases)
[![Docker Pulls](https://img.shields.io/docker/pulls/jeffrysteegmans/sendhub)](https://hub.docker.com/r/jeffrysteegmans/sendhub)
[![Docker Image Version](https://img.shields.io/docker/v/jeffrysteegmans/sendhub?sort=semver&label=Docker%20Hub)](https://hub.docker.com/r/jeffrysteegmans/sendhub)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/github/license/JeffrySteegmans/SendHub)](LICENSE)

SendHub is a .NET automation tool that monitors a folder for new files and automatically sends them as email attachments to configured recipients. It's designed to streamline file distribution workflows with minimal configuration.

## 📋 Overview

SendHub watches a specified folder for new files and automatically emails them to configured recipients using SMTP. It includes a web-based configuration interface accessible in your browser. This makes it perfect for automating document workflows, reports distribution, and file sharing processes.

## ✨ Features

### Implemented

- **Folder Monitoring**: Continuously watches a configured folder for new files using two complementary mechanisms:
  - **Real-time detection** via `FileSystemWatcher` (instant, works on native Linux)
  - **Polling fallback** that re-scans the folder every 30 seconds (configurable) — this is the primary detection path when running in Docker on Windows, where `FileSystemWatcher` cannot receive change events from the Windows host due to a WSL2/inotify limitation
- **Email Delivery**: Automatically sends detected files as email attachments via SMTP
- **File Archiving**: Moves processed files to a configurable destination folder (with automatic conflict resolution)
- **Idempotency Tracking**: Persists processed file records to SQLite database so files are never sent twice after a restart (with automatic migration from legacy JSON tracking)
- **Web-Based Configuration**: A Blazor web interface (accessible on port 8080) for managing all settings without editing config files
- **Database-Driven Configuration**: All settings are stored in SQLite and hot-reloaded at runtime — no restart required after changes

### 🚀 Planned Features

- **Activity Logging**: View send logs and activity history through the web interface
- **Multi-Channel Distribution**: Send files to multiple platforms:
  - Microsoft Teams
  - Slack
  - Custom Webhooks
  - Additional email recipients

## 🔧 Configuration

All settings (watch folder, SMTP, polling interval, etc.) are configured through the **web interface** at `http://localhost:8080` and stored in a SQLite database. No config files need to be edited for normal use.

The only setting that must be provided before the app can start is the **database path** — everything else is managed via the web UI.

### Database path

In `appsettings.json` (or via environment variable):

```json
{
  "SendHub": {
    "Database": {
      "Path": "D:\\SendHub\\sendhub.db"
    }
  }
}
```

Or as an environment variable:

```bash
SendHub__Database__Path=/data/db/sendhub.db
```

## 🚀 Installation

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/JeffrySteegmans/SendHub.git
   cd SendHub
   ```

2. Build the application:

   ```bash
   dotnet build
   ```

3. Configure the application (see Configuration section above)

4. Run the application:

   ```bash
   dotnet run
   ```

## 🐳 Docker

SendHub can run as a Docker container, which is the recommended deployment method for production use.

### Docker Prerequisites

- [Docker Engine](https://docs.docker.com/get-docker/) 24.0 or later
- [Docker Compose](https://docs.docker.com/compose/install/) v2 (included with Docker Desktop)

### Quick Start with Docker Compose

1. Clone the repository:

   ```bash
   git clone https://github.com/JeffrySteegmans/SendHub.git
   cd SendHub
   ```

2. Create a `.env` file from the example:

   ```bash
   cp .env.example .env
   ```

3. Edit `.env` with your scan folder path:

   ```env
   SCAN_FOLDER_HOST_PATH=/path/to/your/scan/folder
   ```

4. Start the container:

   ```bash
   docker compose up -d
   ```

5. Open the web interface at `http://localhost:8080` to configure SendHub.

6. View logs:

   ```bash
   docker compose logs -f sendhub
   ```

### Running with Docker directly

```bash
docker build -t sendhub .

docker run -d \
  --name sendhub \
  --restart unless-stopped \
  -p 8080:8080 \
  -v /path/to/scan/folder:/data/scan \
  -v sendhub-db:/data/db \
  sendhub
```

Then open `http://localhost:8080` to configure SendHub via the web interface.

### Docker on Windows

When running on **Docker Desktop for Windows**, `FileSystemWatcher` cannot detect files dropped from Windows Explorer into a bind-mounted folder. This is a known WSL2 limitation: the Linux container's `inotify` subsystem does not receive change events for writes originating from the Windows host.

SendHub works around this automatically via its **polling fallback**: it re-scans the watch folder every `PollingIntervalSeconds` (default: 30 s) and picks up any files that `FileSystemWatcher` missed. No extra configuration is required — just be aware that detection latency is up to 30 seconds instead of instant when running on Windows.

To reduce the polling interval (e.g. for faster testing), set:

```bash
-e SendHub__PollingIntervalSeconds=5
```

### Volume Reference

| Container path | Purpose | Recommended mount |
| --- | --- | --- |
| `/data/scan` | Folder monitored for new files. Processed files are moved to `/data/scan/Processed`. | Bind mount to host scan folder |
| `/data/db` | Stores `sendhub.db` SQLite database (processed file tracking + all settings). | Named Docker volume |

### Environment Variable Reference

All application settings (watch folder, SMTP, etc.) are managed through the web UI. The only environment variable needed at container startup is the database path.

| Variable | Required | Default | Description |
| --- | --- | --- | --- |
| `SendHub__Database__Path` | No | `/data/db/sendhub.db` | Path to the SQLite database (stores all settings and tracking data) |

---

## 📖 Usage

1. Start SendHub with your configuration
2. Place files in the monitored folder
3. SendHub will automatically detect new files and send them via email
4. Monitor the logs for delivery status

## 🛠️ Development

### Building from Source

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📧 Support

For issues, questions, or suggestions, please open an issue on the [GitHub repository](https://github.com/JeffrySteegmans/SendHub/issues).

## 🗺️ Roadmap

- [x] MVP: Folder monitoring and email delivery
- [x] File archiving (move to destination folder with conflict resolution)
- [x] Idempotency tracking (SQLite database with automatic JSON migration)
- [x] Docker image support
- [x] SQLite database for persistent tracking (with automatic migration from legacy JSON)
- [x] Database-driven configuration (settings stored in SQLite, hot-reloaded at runtime)
- [x] Web-based configuration interface (Blazor, port 8080)
- [ ] Activity logging and history
- [ ] Microsoft Teams integration
- [ ] Slack integration
- [ ] Webhook support
- [ ] Multiple recipient support

---

Made with ❤️ by [Jeffry Steegmans](https://github.com/JeffrySteegmans)

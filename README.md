# SendHub

[![Release Docker Image](https://github.com/JeffrySteegmans/SendHub/actions/workflows/release.yml/badge.svg)](https://github.com/JeffrySteegmans/SendHub/actions/workflows/release.yml)
[![GitHub Release](https://img.shields.io/github/v/release/JeffrySteegmans/SendHub)](https://github.com/JeffrySteegmans/SendHub/releases)
[![Docker Pulls](https://img.shields.io/docker/pulls/jeffrysteegmans/sendhub)](https://hub.docker.com/r/jeffrysteegmans/sendhub)
[![Docker Image Version](https://img.shields.io/docker/v/jeffrysteegmans/sendhub?sort=semver&label=Docker%20Hub)](https://hub.docker.com/r/jeffrysteegmans/sendhub)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/github/license/JeffrySteegmans/SendHub)](LICENSE)

SendHub is a .NET automation tool that monitors a folder for new files and automatically sends them as email attachments to configured recipients. It's designed to streamline file distribution workflows with minimal configuration.

## 📋 Overview

SendHub watches a specified folder for new files and automatically emails them to configured recipients using SMTP. This makes it perfect for automating document workflows, reports distribution, and file sharing processes.

## ✨ Features

### MVP (Implemented)

- **Folder Monitoring**: Continuously watches a configured folder for new files using two complementary mechanisms:
  - **Real-time detection** via `FileSystemWatcher` (instant, works on native Linux)
  - **Polling fallback** that re-scans the folder every 30 seconds (configurable) — this is the primary detection path when running in Docker on Windows, where `FileSystemWatcher` cannot receive change events from the Windows host due to a WSL2/inotify limitation
- **Email Delivery**: Automatically sends detected files as email attachments via SMTP
- **File Archiving**: Moves processed files to a configurable destination folder (with automatic conflict resolution)
- **Idempotency Tracking**: Persists processed file records to SQLite database so files are never sent twice after a restart (with automatic migration from legacy JSON tracking)
- **Flexible Configuration**: Configure via `appsettings.json`, user secrets, or environment variables:
  - Folder paths (watch folder and destination folder)
  - SMTP server settings (host, port, credentials, SSL)
  - Database path for tracking storage

### 🚀 Planned Features

- **Web-Based Configuration**: A web interface for easy configuration management
  - Configure folder to watch
  - Manage email recipients
  - Configure SMTP server settings
- **Activity Logging**: View send logs and activity history through the web interface
- **Multi-Channel Distribution**: Send files to multiple platforms:
  - Microsoft Teams
  - Slack
  - Custom Webhooks
  - Additional email recipients

## 🔧 Configuration

SendHub can be configured using either `appsettings.json` or environment variables.

### Using appsettings.json

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

### Using Environment Variables

```bash
SendHub__WatchFolder=D:\ScanFolder
SendHub__DestinationFolder=D:\ScanFolder\Processed
SendHub__Database__Path=D:\SendHub\sendhub.db
SendHub__Email__Smtp__Host=smtp.gmail.com
SendHub__Email__Smtp__Port=587
SendHub__Email__Smtp__Username=your-email@gmail.com
SendHub__Email__Smtp__Password=your-app-password
SendHub__Email__Smtp__EnableSsl=true
SendHub__Email__Smtp__From=sendhub@example.com
SendHub__Email__Smtp__To=recipient@example.com
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

3. Edit `.env` with your SMTP settings and scan folder path:

   ```env
   SMTP_HOST=smtp.gmail.com
   SMTP_PORT=587
   SMTP_USERNAME=your-email@gmail.com
   SMTP_PASSWORD=your-app-password
   SMTP_FROM=sendhub@example.com
   SMTP_TO=recipient@example.com
   SCAN_FOLDER_HOST_PATH=/path/to/your/scan/folder
   ```

4. Start the container:

   ```bash
   docker compose up -d
   ```

5. View logs:

   ```bash
   docker compose logs -f sendhub
   ```

### Running with Docker directly

```bash
docker build -t sendhub .

docker run -d \
  --name sendhub \
  --restart unless-stopped \
  -v /path/to/scan/folder:/data/scan \
  -v sendhub-data:/data/db \
  -e SendHub__Email__Smtp__Host=smtp.gmail.com \
  -e SendHub__Email__Smtp__Port=587 \
  -e SendHub__Email__Smtp__Username=your-email@gmail.com \
  -e SendHub__Email__Smtp__Password=your-app-password \
  -e SendHub__Email__Smtp__From=sendhub@example.com \
  -e SendHub__Email__Smtp__To=recipient@example.com \
  sendhub
```

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
| `/data/db` | Stores `sendhub.db` SQLite database to prevent re-sending files after container restart. | Named Docker volume |

### Environment Variable Reference

| Variable | Required | Default | Description |
| --- | --- | --- | --- |
| `SendHub__WatchFolder` | No | `/data/scan` | Folder to monitor for new files |
| `SendHub__DestinationFolder` | No | `/data/scan/Processed` | Where processed files are archived |
| `SendHub__Database__Path` | No | `/data/db/sendhub.db` | Path to SQLite database for tracking processed files |
| `SendHub__PollingIntervalSeconds` | No | `30` | Interval in seconds between folder re-scans. Acts as a fallback when `FileSystemWatcher` events are not received (e.g. Docker on Windows) |
| `SendHub__Email__Smtp__Host` | Yes | — | SMTP server hostname |
| `SendHub__Email__Smtp__Port` | Yes | — | SMTP server port (587 for STARTTLS) |
| `SendHub__Email__Smtp__Username` | No | — | SMTP username (omit for anonymous relay) |
| `SendHub__Email__Smtp__Password` | No | — | SMTP password |
| `SendHub__Email__Smtp__EnableSsl` | No | `true` | Use SSL/TLS for SMTP |
| `SendHub__Email__Smtp__From` | Yes | — | Sender email address |
| `SendHub__Email__Smtp__To` | Yes | — | Recipient email address |

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
- [ ] Database-driven configuration (settings stored in SQLite)
- [ ] Web-based configuration interface
- [ ] Activity logging and history
- [ ] Microsoft Teams integration
- [ ] Slack integration
- [ ] Webhook support
- [ ] Multiple recipient support

---

Made with ❤️ by [Jeffry Steegmans](https://github.com/JeffrySteegmans)

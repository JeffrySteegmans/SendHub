# SendHub

SendHub is a .NET automation tool that monitors a folder for new files and automatically sends them as email attachments to configured recipients. It's designed to streamline file distribution workflows with minimal configuration.

## 📋 Overview

SendHub watches a specified folder for new files and automatically emails them to configured recipients using SMTP. This makes it perfect for automating document workflows, reports distribution, and file sharing processes.

## ✨ Features

### MVP (Implemented)

- **Folder Monitoring**: Continuously watches a configured folder for new files using a real-time file system watcher with 3 concurrent worker threads
- **Email Delivery**: Automatically sends detected files as email attachments via SMTP
- **File Archiving**: Moves processed files to a configurable destination folder (with automatic conflict resolution)
- **Idempotency Tracking**: Persists processed file records to JSON so files are never sent twice after a restart
- **Flexible Configuration**: Configure via `appsettings.json`, user secrets, or environment variables:
  - Folder paths (watch folder and destination folder)
  - SMTP server settings (host, port, credentials, SSL)
  - Tracking file path

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
    },
    "Tracking": {
      "FilePath": "D:\\SendHub\\tracking.json"
    }
  }
}
```

### Using Environment Variables

```bash
SendHub_WatchFolder=D:\ScanFolder
SendHub_DestinationFolder=D:\ScanFolder\Processed
SendHub_Email__Smtp__Host=smtp.gmail.com
SendHub_Email__Smtp__Port=587
SendHub_Email__Smtp__Username=your-email@gmail.com
SendHub_Email__Smtp__Password=your-app-password
SendHub_Email__Smtp__EnableSsl=true
SendHub_Email__Smtp__From=sendhub@example.com
SendHub_Email__Smtp__To=recipient@example.com
SendHub_Tracking__FilePath=D:\SendHub\tracking.json
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
  -v sendhub-tracking:/data/tracking \
  -e SendHub__Email__Smtp__Host=smtp.gmail.com \
  -e SendHub__Email__Smtp__Port=587 \
  -e SendHub__Email__Smtp__Username=your-email@gmail.com \
  -e SendHub__Email__Smtp__Password=your-app-password \
  -e SendHub__Email__Smtp__From=sendhub@example.com \
  -e SendHub__Email__Smtp__To=recipient@example.com \
  sendhub
```

### Volume Reference

| Container path | Purpose | Recommended mount |
| --- | --- | --- |
| `/data/scan` | Folder monitored for new files. Processed files are moved to `/data/scan/Processed`. | Bind mount to host scan folder |
| `/data/tracking` | Stores `tracking.json` to prevent re-sending files after container restart. | Named Docker volume |

### Environment Variable Reference

| Variable | Required | Default | Description |
| --- | --- | --- | --- |
| `SendHub__WatchFolder` | No | `/data/scan` | Folder to monitor for new files |
| `SendHub__DestinationFolder` | No | `/data/scan/Processed` | Where processed files are archived |
| `SendHub__Tracking__FilePath` | No | `/data/tracking/tracking.json` | Path to idempotency tracking file |
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
- [x] Idempotency tracking (JSON persistence, survives restarts)
- [x] Docker image support
- [ ] Web-based configuration interface
- [ ] Activity logging and history
- [ ] Microsoft Teams integration
- [ ] Slack integration
- [ ] Webhook support
- [ ] Multiple recipient support

---

Made with ❤️ by [Jeffry Steegmans](https://github.com/JeffrySteegmans)

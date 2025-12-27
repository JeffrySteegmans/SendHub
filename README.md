# SendHub

SendHub is a .NET automation tool that monitors a folder for new files and automatically sends them as email attachments to configured recipients. It's designed to streamline file distribution workflows with minimal configuration.

## 📋 Overview

SendHub watches a specified folder for new files and automatically emails them to configured recipients using SMTP. This makes it perfect for automating document workflows, reports distribution, and file sharing processes.

## ✨ Features

### MVP (Current Version)

- **Folder Monitoring**: Continuously watches a configured folder for new files
- **Email Delivery**: Automatically sends detected files as email attachments to configured recipients
- **Flexible Configuration**: Configure via `appsettings.json` or environment variables:
  - Folder path to monitor
  - Recipient email addresses
  - SMTP server settings

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
    "WatchFolder": "/path/to/watch",
    "Email": {
      "To": "recipient@example.com",
      "Smtp": {
        "Host": "smtp.example.com",
        "Port": 587,
        "Username": "your-username",
        "Password": "your-password",
        "EnableSsl": true
      }
    }
  }
}
```

### Using Environment Variables

```bash
SENDHUB_WATCHFOLDER=/path/to/watch
SENDHUB_EMAIL__TO=recipient@example.com
SENDHUB_EMAIL__SMTP__HOST=smtp.example.com
SENDHUB_EMAIL__SMTP__PORT=587
SENDHUB_EMAIL__SMTP__USERNAME=your-username
SENDHUB_EMAIL__SMTP__PASSWORD=your-password
SENDHUB_EMAIL__SMTP__ENABLESSL=true
```

## 🚀 Installation

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later

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
- [ ] Web-based configuration interface
- [ ] Activity logging and history
- [ ] Microsoft Teams integration
- [ ] Slack integration
- [ ] Webhook support
- [ ] Multiple recipient support

---

Made with ❤️ by [Jeffry Steegmans](https://github.com/JeffrySteegmans)

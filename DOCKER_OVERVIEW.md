## SendHub

**SendHub** is a lightweight .NET daemon that monitors a folder for new files and automatically emails them as attachments to configured recipients — no code required, just drop files and go.

### What it does

- **Watches a folder** for new files using `FileSystemWatcher` with a polling fallback (every 30 s by default)
- **Emails files** automatically as attachments via any SMTP server (Gmail, Office 365, self-hosted, etc.)
- **Archives processed files** to a `Processed` subfolder with automatic conflict resolution
- **Tracks processed files** in a persistent JSON file so files are never sent twice after a container restart

### Quick start

```bash
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
  jeffrysteegmans/sendhub
```

### Volumes

| Path | Purpose |
|---|---|
| `/data/scan` | Folder monitored for new files. Processed files move to `/data/scan/Processed`. |
| `/data/tracking` | Stores `tracking.json` to prevent re-sending files after restarts. |

### Key environment variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `SendHub__Email__Smtp__Host` | Yes | — | SMTP server hostname |
| `SendHub__Email__Smtp__Port` | Yes | — | SMTP port (587 for STARTTLS) |
| `SendHub__Email__Smtp__Username` | No | — | SMTP username |
| `SendHub__Email__Smtp__Password` | No | — | SMTP password |
| `SendHub__Email__Smtp__EnableSsl` | No | `true` | Enable SSL/TLS |
| `SendHub__Email__Smtp__From` | Yes | — | Sender email address |
| `SendHub__Email__Smtp__To` | Yes | — | Recipient email address |
| `SendHub__PollingIntervalSeconds` | No | `30` | Polling interval in seconds |

### Docker on Windows

When running on Docker Desktop for Windows, `FileSystemWatcher` cannot receive change events from the Windows host (WSL2/inotify limitation). SendHub handles this automatically via its polling fallback — files are picked up within `PollingIntervalSeconds` with no extra configuration needed.

### Source & documentation

[github.com/JeffrySteegmans/SendHub](https://github.com/JeffrySteegmans/SendHub)

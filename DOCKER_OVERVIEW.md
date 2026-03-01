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

### Docker Compose example

```yaml
services:
  sendhub:
    image: jeffrysteegmans/sendhub
    restart: unless-stopped
    volumes:
      - /path/to/scan/folder:/data/scan
      - sendhub-tracking:/data/tracking
    environment:
      SendHub__Email__Smtp__Host: smtp.gmail.com
      SendHub__Email__Smtp__Port: 587
      SendHub__Email__Smtp__Username: your-email@gmail.com
      SendHub__Email__Smtp__Password: your-app-password
      SendHub__Email__Smtp__From: sendhub@example.com
      SendHub__Email__Smtp__To: recipient@example.com

volumes:
  sendhub-tracking:
```

Save this as `compose.yaml`, set your values, then run:

```bash
docker compose up -d
```

### Volume permissions (Synology NAS and similar)

The container runs as a non-root user. When you mount a host folder, the container process must have write access to it. If you see `UnauthorizedAccessException: Access to the path '/data/scan/Processed' is denied`, the container user does not match the folder owner on the host.

**Fix:** add a `user` field to the compose service with the UID and GID of the host user that owns the scan folder.

1. SSH into your NAS and run `id` to find your UID and GID:

   ```text
   uid=1026(myuser) gid=100(users)
   ```

2. Add `user: "1026:100"` to the service (replace with your actual values):

   ```yaml
   services:
     sendhub:
       image: jeffrysteegmans/sendhub
       user: "1026:100"
       ...
   ```

### Docker on Windows

When running on Docker Desktop for Windows, `FileSystemWatcher` cannot receive change events from the Windows host (WSL2/inotify limitation). SendHub handles this automatically via its polling fallback — files are picked up within `PollingIntervalSeconds` with no extra configuration needed.

### Source & documentation

[github.com/JeffrySteegmans/SendHub](https://github.com/JeffrySteegmans/SendHub)

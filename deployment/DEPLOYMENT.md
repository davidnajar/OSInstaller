# OS Installer - Deployment Guide

## Production Deployment

### Prerequisites

- Linux system with systemd
- .NET 10 Runtime
- Chromium browser
- X11 or Wayland display server

### Installation Steps

1. **Build the application**

```bash
cd OSInstaller
npm install
npm run css:build
cd ..
dotnet publish -c Release -o /opt/osinstaller
```

2. **Create installer user**

```bash
sudo useradd -r -m -d /var/lib/installer installer
```

3. **Create spec directories**

```bash
sudo mkdir -p /usr/share/installer/spec.d
sudo mkdir -p /etc/installer/spec.d
sudo mkdir -p /var/lib/installer/spec.d
sudo chown -R installer:installer /var/lib/installer
```

4. **Copy your spec files**

```bash
sudo cp your-specs/*.json /usr/share/installer/spec.d/
```

5. **Install systemd services**

```bash
sudo cp deployment/osinstaller.service /etc/systemd/system/
sudo cp deployment/osinstaller-kiosk.service /etc/systemd/system/
sudo systemctl daemon-reload
```

6. **Enable and start services**

```bash
sudo systemctl enable osinstaller.service
sudo systemctl start osinstaller.service
sudo systemctl enable osinstaller-kiosk.service
sudo systemctl start osinstaller-kiosk.service
```

### Verify Installation

Check service status:
```bash
sudo systemctl status osinstaller.service
sudo systemctl status osinstaller-kiosk.service
```

View logs:
```bash
sudo journalctl -u osinstaller.service -f
sudo journalctl -u osinstaller-kiosk.service -f
```

### Testing Specs

Test spec merging:
```bash
curl http://localhost:5070/api/spec/diagnostics | jq
```

Get unified spec:
```bash
curl http://localhost:5070/api/spec/unified | jq
```

### Output Location

The final configuration is saved to:
```
/var/lib/installer/output.json
```

### Customization

#### Change listen port

Edit `/etc/systemd/system/osinstaller.service`:
```ini
ExecStart=/usr/bin/dotnet /opt/osinstaller/OSInstaller.dll --urls=http://localhost:YOUR_PORT
```

And update the kiosk service to match.

#### Modify kiosk Chromium options

Edit `/etc/systemd/system/osinstaller-kiosk.service` to add/remove Chromium flags.

### Troubleshooting

#### Backend not starting
- Check .NET 10 runtime is installed: `dotnet --version`
- Check file permissions: `ls -la /opt/osinstaller`
- View detailed logs: `sudo journalctl -u osinstaller.service -n 100`

#### Kiosk not displaying
- Verify DISPLAY environment variable is correct
- Check X11 permissions for installer user
- Test Chromium manually: `chromium --kiosk http://localhost:5070`

#### Specs not loading
- Verify spec files are valid JSON
- Check directory permissions: `ls -la /usr/share/installer/spec.d`
- Check merge diagnostics: `curl http://localhost:5070/api/spec/diagnostics`

## Security Considerations

1. The installer runs on localhost only by default
2. Create a dedicated user with minimal permissions
3. Spec directories should be read-only for the installer user
4. Output directory should be write-only for the installer user
5. Consider using AppArmor or SELinux policies to further restrict the service

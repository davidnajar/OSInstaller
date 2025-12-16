# OS Installer - Dynamic Installer Wizard

A lightweight installer wizard that runs locally on Linux and dynamically generates its UI from JSON specs. Built with Blazor WebAssembly and DaisyUI.

## Features

- **Dynamic UI Generation**: Wizard pages and fields are generated from JSON specification files
- **Layered Spec System**: Multiple layers (OS/Kairos/container/plugins) can contribute specs
- **Spec Merging**: All contributions are merged into a single unified wizard
- **Client-Side Validation**: Form validation happens in the browser (text, number, bool, IP, regex patterns)
- **State Persistence**: Wizard progress is saved and survives browser restarts
- **Modern UI**: Built with Tailwind CSS and DaisyUI components
- **Airgapped Support**: All CSS/JS assets are embedded (no CDN dependencies)

## Architecture

### Backend (ASP.NET Core)
- Discovers and loads spec contributions from multiple directories
- Validates and merges specs into a unified spec
- Provides REST APIs for spec retrieval and wizard state management
- Generates final output JSON using JSONPath

### Frontend (Blazor WebAssembly)
- Renders wizard dynamically from unified spec
- Client-side form rendering and validation
- DaisyUI components for forms, steps, alerts, and progress
- Persists wizard state via backend API

## Spec Directory Structure

Spec files are loaded from these directories (in order):

**Development:**
- `{ProjectRoot}/example-specs/`

**Production:**
- `/usr/share/installer/spec.d/` (base OS / vendor)
- `/etc/installer/spec.d/` (admin overrides)
- `/var/lib/installer/spec.d/` (runtime / plugins)

## Spec File Format

### Basic Spec Contribution

```json
{
  "contribId": "net.base",
  "priority": 100,
  "pages": [
    {
      "id": "network",
      "title": "Network",
      "order": 10,
      "fields": [
        {
          "id": "net.hostname",
          "label": "Hostname",
          "type": "text",
          "required": true,
          "jsonPath": "$.network.hostname",
          "errorMessage": "Hostname is required"
        }
      ]
    }
  ],
  "outputTemplatePatch": {
    "network": {
      "hostname": ""
    }
  }
}
```

### Page Patch (Adding fields to existing pages)

```json
{
  "contribId": "vpn.addon",
  "priority": 300,
  "pagePatches": [
    {
      "pageId": "network",
      "insertFieldsAfter": "net.hostname",
      "fields": [
        {
          "id": "vpn.enabled",
          "type": "bool",
          "label": "Enable VPN",
          "jsonPath": "$.vpn.enabled"
        }
      ]
    }
  ],
  "outputTemplatePatch": {
    "vpn": { "enabled": false }
  }
}
```

## Field Types

- `text`: Text input
- `number`: Number input with optional min/max
- `bool`: Checkbox
- `ip`: IP address with validation
- `regex`: Text with regex pattern validation

## Merge Rules

- Contributions are applied in ascending priority order
- Page IDs must be unique (use `pagePatches` to modify existing pages)
- Field IDs must be globally unique (namespace recommended: `net.hostname`)
- Output templates are deep-merged

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js (for Tailwind CSS build)

### Build

```bash
# Build CSS
cd OSInstaller
npm install
npm run css:build

# Build application
cd ..
dotnet build
```

### Run

```bash
dotnet run --project OSInstaller/OSInstaller.csproj
```

The application will be available at http://localhost:5070

### Development

To rebuild CSS during development:

```bash
cd OSInstaller
npm run css:watch
```

## API Endpoints

- `GET /api/spec/unified` - Get merged spec
- `GET /api/spec/diagnostics` - Get merge diagnostics
- `POST /api/spec/reload` - Reload spec cache
- `GET /api/wizard/state` - Get wizard state
- `POST /api/wizard/state` - Save wizard state
- `DELETE /api/wizard/state` - Clear wizard state
- `POST /api/wizard/generate` - Generate final output JSON

## Output

The final configuration is saved to:
- Development: `{ProjectRoot}/output.json`
- Production: `/var/lib/installer/output.json`

## Example Specs

See the `example-specs/` directory for examples:
- `base.json`: Base system and network configuration
- `kairos.json`: Kairos-specific settings
- `vpn-addon.json`: VPN addon that patches the network page

## Linux Kiosk Mode

For production deployment, the app can be launched in Chromium kiosk mode:

```bash
chromium --kiosk --app=http://localhost:5070
```

Configure systemd to start both the backend and Chromium on boot.

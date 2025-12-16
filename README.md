# OSInstaller

A lightweight, dynamic installer wizard that runs locally on Linux and generates its UI from JSON specifications. Built with Blazor WebAssembly and DaisyUI for a modern, responsive installer experience.

## Features

- **Dynamic UI Generation**: Wizard pages and fields are generated from JSON specification files
- **Layered Spec System**: Multiple layers (OS/Kairos/container/plugins) can contribute specs that are merged automatically
- **Client-Side Validation**: Form validation happens in the browser (text, number, bool, IP, regex patterns)
- **State Persistence**: Wizard progress is saved and survives browser restarts
- **Modern UI**: Built with Tailwind CSS and DaisyUI components
- **Airgapped Support**: All CSS/JS assets are embedded (no CDN dependencies)
- **Linux Kiosk Mode**: Designed to run in Chromium kiosk mode via systemd

## Architecture

### Backend (ASP.NET Core)
- Discovers and loads spec contributions from multiple directories
- Validates and merges specs into a unified spec with conflict detection
- Provides REST APIs for spec retrieval and wizard state management
- Generates final output JSON using JSONPath

### Frontend (Blazor WebAssembly)
- Renders wizard dynamically from unified spec
- Client-side form rendering and validation
- DaisyUI components for forms, steps, alerts, and progress
- Persists wizard state via backend API

## Project Structure

- `OSInstaller/` - Main ASP.NET Core web application project (backend)
- `OSInstaller.Client/` - Blazor WebAssembly client project (frontend)
- `example-specs/` - Example specification files
- `deployment/` - Systemd service files and deployment guide

## Getting Started

### Prerequisites

- .NET 10 SDK
- Node.js (for Tailwind CSS build)

### Build the Project

```bash
# Install npm dependencies and build CSS
cd OSInstaller
npm install
npm run css:build

# Build the application
cd ..
dotnet build
```

### Run the Application

```bash
dotnet run --project OSInstaller/OSInstaller.csproj
```

The application will be available at http://localhost:5070

For CSS development with hot reload:
```bash
cd OSInstaller
npm run css:watch
```

## Documentation

- **[INSTALLER_README.md](INSTALLER_README.md)** - Detailed guide on spec file format and usage
- **[deployment/DEPLOYMENT.md](deployment/DEPLOYMENT.md)** - Production deployment guide for Linux with systemd

## Quick Example

Create a spec file in `OSInstaller/example-specs/`:

```json
{
  "contribId": "my.custom",
  "priority": 100,
  "pages": [
    {
      "id": "mypage",
      "title": "My Configuration",
      "order": 10,
      "fields": [
        {
          "id": "my.field",
          "label": "My Field",
          "type": "text",
          "required": true,
          "jsonPath": "$.myConfig.field"
        }
      ]
    }
  ],
  "outputTemplatePatch": {
    "myConfig": { "field": "" }
  }
}
```

The wizard will automatically include this page when you reload the spec.

## API Endpoints

- `GET /api/spec/unified` - Get merged spec
- `GET /api/spec/diagnostics` - Get merge diagnostics
- `POST /api/spec/reload` - Reload spec cache
- `GET /api/wizard/state` - Get wizard state
- `POST /api/wizard/state` - Save wizard state
- `POST /api/wizard/generate` - Generate final output JSON

## Output

The final configuration is saved to:
- **Development**: `{ProjectRoot}/output.json`
- **Production**: `/var/lib/installer/output.json`

## Production Deployment

For Linux systemd deployment with Chromium kiosk mode, see [deployment/DEPLOYMENT.md](deployment/DEPLOYMENT.md).

## License

See LICENSE file for details.
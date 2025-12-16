# OS Installer - Dynamic Installer Wizard

A lightweight installer wizard that runs locally on Linux and dynamically generates its UI from JSON specs. Built with Blazor WebAssembly and DaisyUI.

## Features

- **Dynamic UI Generation**: Wizard pages and fields are generated from JSON specification files
- **Feature-Gated Pages**: Optional pages with Yes/No toggles that can be skipped entirely
- **Password Fields**: Automatic confirmation fields and visibility toggles for password inputs
- **Keyboard Navigation**: Full keyboard support (Tab, Enter=Next, Alt+Backspace=Previous)
- **Summary Page**: Review all configuration before installation with safe password masking
- **CLI Configuration**: Runtime configuration via command-line arguments
- **Layered Spec System**: Multiple layers (OS/Kairos/container/plugins) can contribute specs
- **Spec Merging**: All contributions are merged into a single unified wizard
- **Client-Side Validation**: Form validation happens in the browser (text, number, bool, IP, regex, password matching)
- **State Persistence**: Wizard progress is saved and survives browser restarts
- **Modern UI**: Built with Tailwind CSS and DaisyUI components
- **Airgapped Support**: All CSS/JS assets are embedded (no CDN dependencies)

## Architecture

### Backend (ASP.NET Core)
- Discovers and loads spec contributions from multiple directories
- Validates and merges specs into a unified spec
- Provides REST APIs for spec retrieval and wizard state management
- Generates final output JSON using JSONPath
- Supports CLI arguments for runtime configuration

### Frontend (Blazor WebAssembly)
- Renders wizard dynamically from unified spec
- Client-side form rendering and validation
- DaisyUI components for forms, steps, alerts, and progress
- Persists wizard state via backend API
- Keyboard-only navigation support
- Summary page with safe value display

## Command-Line Arguments

The installer supports the following CLI arguments:

```bash
OSInstaller [options]

Options:
  --spec-dir <path>    Specification directory (can be specified multiple times)
  --output <path>      Output JSON file path
  --help, -h           Show this help message

Example:
  OSInstaller --spec-dir /usr/share/installer/spec.d \
              --spec-dir /etc/installer/spec.d \
              --output /var/lib/installer/config.json
```

## Spec Directory Structure

Spec files are loaded from these directories (in order):

**Development:**
- `{ProjectRoot}/example-specs/`

**Production (default):**
- `/usr/share/installer/spec.d/` (base OS / vendor)
- `/etc/installer/spec.d/` (admin overrides)
- `/var/lib/installer/spec.d/` (runtime / plugins)

**Custom (via CLI):**
- Any directories specified with `--spec-dir` flags (in order provided)

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

### Feature-Gated Page (Optional Page)

Pages can be made optional with a feature toggle that allows users to enable/disable the entire page:

```json
{
  "contribId": "vpn.feature",
  "priority": 200,
  "pages": [
    {
      "id": "vpn",
      "title": "VPN Configuration",
      "order": 30,
      "description": "Configure VPN connection settings",
      "featureToggle": {
        "label": "Enable VPN",
        "default": false
      },
      "fields": [
        {
          "id": "vpn.server",
          "label": "VPN Server",
          "type": "text",
          "required": true,
          "jsonPath": "$.vpn.server"
        }
      ]
    }
  ],
  "outputTemplatePatch": {
    "vpn": {
      "enabled": false,
      "server": ""
    }
  }
}
```

When a page has a `featureToggle`:
- A Yes/No toggle appears at the top of the page
- If disabled, all fields are hidden and validation is skipped
- The summary page shows the feature as "Disabled"
- The toggle state is saved in the wizard state

### Password Field Example

Password fields automatically render with confirmation and visibility toggle:

```json
{
  "id": "admin.password",
  "label": "Admin Password",
  "type": "password",
  "required": true,
  "jsonPath": "$.admin.password",
  "placeholder": "Enter a strong password"
}
```

Password fields automatically include:
- A confirmation field (labeled "Confirm [Label]")
- Validation to ensure both fields match
- An eye icon to toggle visibility for both fields
- Password masking in the summary page (shown as ••••••••)

## Field Types

- `text`: Text input
- `number`: Number input with optional min/max
- `bool`: Checkbox toggle
- `password`: Password input with automatic confirmation and visibility toggle
- `ip`: IP address with validation
- `regex`: Text with regex pattern validation

## Keyboard Shortcuts

The wizard supports full keyboard navigation:

- **Tab / Shift+Tab**: Navigate between form fields (browser default)
- **Enter**: Proceed to next page (when current page is valid)
- **Alt+Backspace**: Go back to previous page
- **Mouse clicks**: Also supported for all interactions

## Summary Page

Before finalizing the installation, a summary page is displayed showing:

- All configured pages and their status (Enabled/Disabled)
- All field values except:
  - Password fields (shown as ••••••••)
  - Empty/unset fields
- Clear indication of which features are enabled or disabled
- "Back to Edit" button to modify configuration
- "Confirm and Install" button to proceed

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

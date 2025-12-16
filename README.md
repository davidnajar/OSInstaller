# OSInstaller

A .NET 10 ASP.NET Core Blazor Web App with WebAssembly rendering mode.

## Features

- **Framework**: .NET 10
- **Rendering Mode**: WebAssembly (runs in the browser)
- **Architecture**: Minimal/Empty template
- **No Server-side rendering**: Pure client-side execution
- **No Interactive components**: Static components only

## Project Structure

- `OSInstaller/` - Main ASP.NET Core web application project
- `OSInstaller.Client/` - Blazor WebAssembly client project

## Getting Started

### Prerequisites

- .NET 10 SDK

### Build the Project

```bash
dotnet build
```

### Run the Application

```bash
dotnet run --project OSInstaller/OSInstaller.csproj
```

The application will be available at:
- HTTP: http://localhost:5068
- HTTPS: https://localhost:7020

## Development

The application uses:
- Blazor WebAssembly for client-side rendering
- Minimal template with basic pages (Home, Error, NotFound)
- Default layout with error UI
# Stage 1: Build frontend assets
FROM node:22-alpine AS frontend-build
WORKDIR /app/OSInstaller
COPY OSInstaller/package.json ./
RUN npm install
COPY OSInstaller/Styles ./Styles
COPY OSInstaller/tailwind.config.js ./
COPY OSInstaller/postcss.config.js ./
RUN npm run css:build

# Stage 2: Build .NET application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY OSInstaller.sln ./
COPY OSInstaller/OSInstaller.csproj ./OSInstaller/
COPY OSInstaller.Client/OSInstaller.Client.csproj ./OSInstaller.Client/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY OSInstaller/ ./OSInstaller/
COPY OSInstaller.Client/ ./OSInstaller.Client/

# Copy built CSS from frontend stage
COPY --from=frontend-build /app/OSInstaller/wwwroot/app.css ./OSInstaller/wwwroot/

# Build and publish
WORKDIR /src/OSInstaller
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install Chromium for kiosk mode (optional, can be removed if not needed in container)
# RUN apt-get update && apt-get install -y chromium && rm -rf /var/lib/apt/lists/*

# Create installer user and directories
RUN useradd -r -m -d /var/lib/installer installer && \
    mkdir -p /usr/share/installer/spec.d && \
    mkdir -p /etc/installer/spec.d && \
    mkdir -p /var/lib/installer/spec.d && \
    chown -R installer:installer /var/lib/installer

# Copy published application
COPY --from=build /app/publish .

# Set user
USER installer

# Expose port
EXPOSE 5070

# Set environment
ENV ASPNETCORE_URLS=http://+:5070
ENV ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["dotnet", "OSInstaller.dll"]
CMD ["--spec-dir", "/usr/share/installer/spec.d", "--spec-dir", "/etc/installer/spec.d", "--spec-dir", "/var/lib/installer/spec.d", "--output", "/var/lib/installer/output.json"]

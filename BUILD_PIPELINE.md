# Build Pipeline Configuration

This document describes the build pipeline and required secrets configuration.

## Pipeline Overview

The build pipeline is triggered on pushes to the `main` branch and performs the following:

1. **Version Calculation**: Uses GitVersion to calculate semantic version from git history
2. **Docker Build**: Builds a multi-stage Docker image with frontend and backend
3. **Push to Artifactory**: Pushes the image to a private Artifactory Docker registry

## Required GitHub Secrets

Configure the following secrets in your GitHub repository settings (Settings → Secrets and variables → Actions):

| Secret Name | Description | Example |
|-------------|-------------|---------|
| `ARTIFACTORY_REGISTRY` | Your Artifactory Docker registry URL | `mycompany.jfrog.io` |
| `ARTIFACTORY_REPOSITORY` | Repository path in Artifactory | `docker-local` or `my-project/docker` |
| `ARTIFACTORY_USER` | Username for Artifactory authentication | `build-user` |
| `ARTIFACTORY_PASSWORD` | Password or API token for Artifactory | `your-api-token` |

## GitVersion Configuration

The `GitVersion.yml` file configures semantic versioning:

- **Main branch**: Produces release versions (e.g., `1.0.0`, `1.0.1`)
- **Develop branch**: Produces alpha versions (e.g., `1.0.0-alpha.1`)
- **Feature branches**: Produces feature versions (e.g., `1.0.0-feature-xyz.1`)

Version increments are automatic based on commits.

## Docker Image

The Docker image is built in three stages:

1. **Frontend Build**: Installs npm dependencies and builds Tailwind CSS
2. **Backend Build**: Compiles .NET application and publishes artifacts
3. **Runtime**: Creates minimal runtime image with ASP.NET Core runtime

### Image Tags

Two tags are pushed for each build:
- `<registry>/<repository>/osinstaller:<version>` - Specific version (e.g., `1.0.0`)
- `<registry>/<repository>/osinstaller:latest` - Latest main branch build

## Dockerfile

The Dockerfile supports multi-stage builds and creates a production-ready image:

- Based on .NET 10.0 runtime
- Runs as non-root `installer` user
- Exposes port 5070
- Includes predefined spec directories
- Configurable output path

### Running the Container

```bash
docker run -d \
  -p 5070:5070 \
  -v /path/to/specs:/usr/share/installer/spec.d \
  -v /path/to/output:/var/lib/installer \
  <registry>/<repository>/osinstaller:latest
```

## Workflow File

The workflow is defined in `.github/workflows/build-push.yml` and:

- Runs on `ubuntu-latest` runners
- Uses Docker Buildx for efficient builds
- Leverages GitHub Actions cache for Docker layers
- Adds OCI image labels for metadata

## Testing Locally

To test the Docker build locally:

```bash
docker build -t osinstaller:test .
docker run --rm -p 5070:5070 osinstaller:test
```

## Troubleshooting

### Build Fails on Frontend Stage

- Ensure `package.json` is present in `OSInstaller/` directory
- Check that Tailwind CSS configuration files exist

### Build Fails on Backend Stage

- Verify .NET SDK 10.0 is available in the build image
- Check that all `.csproj` files are present

### Authentication Issues

- Verify secrets are correctly configured
- Test Artifactory credentials manually
- Check that the registry URL doesn't include `https://`

## Future Enhancements

Consider adding:
- Pull request builds (without push)
- Security scanning (Trivy, Snyk)
- Multi-architecture builds (ARM64)
- Release notes generation
- Slack/Teams notifications

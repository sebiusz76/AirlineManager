# GitHub Actions Workflows for AirlineManager

This repository contains two CI/CD workflows for building and pushing Docker images to the Sebiusz Registry.

## Workflows

### 1. `release.yml` - Production Release (main branch)

**Trigger:** Push to `main` branch

**What it does:**
- ✅ Builds the solution in **Release** configuration
- ✅ Runs all tests
- ✅ Creates Docker image with version tag and `latest` tag
- ✅ Pushes to `registry.hub.sebiusz.ovh/airlinemanager`

**Image tags:**
- `registry.hub.sebiusz.ovh/airlinemanager:latest`
- `registry.hub.sebiusz.ovh/airlinemanager:{version}` (from git tag)

**OCI Labels:**
- Full production metadata
- Source: https://git.sebiusz.ovh/sebiusz/AirlineManager
- Mirror: https://github.com/sebiusz76/AirlineManager

---

### 2. `develop.yml` - Development Build (develop branch)

**Trigger:** Push to `develop` branch

**What it does:**
- ✅ Builds the solution in **Debug** configuration
- ✅ Runs all tests
- ✅ Creates Docker image with version tag (with `-dev` suffix) and `latest-dev` tag
- ✅ Pushes to `registry.hub.sebiusz.ovh/airlinemanager-dev`

**Image tags:**
- `registry.hub.sebiusz.ovh/airlinemanager-dev:latest-dev`
- `registry.hub.sebiusz.ovh/airlinemanager-dev:{version}-dev` (from git tag)

**OCI Labels:**
- Development build metadata
- Includes branch name reference

---

## Key Differences

| Feature | Release (main) | Develop |
|---------|---------------|---------|
| **Branch** | `main` | `develop` |
| **Build Config** | Release | Debug |
| **Image Name** | `airlinemanager` | `airlinemanager-dev` |
| **Latest Tag** | `latest` | `latest-dev` |
| **Version Suffix** | none | `-dev` |
| **Purpose** | Production deployment | Testing & development |

---

## Required GitHub Secrets

Both workflows require the following secrets to be configured in your repository:

- `REGISTRY_USER` - Username for registry.hub.sebiusz.ovh
- `REGISTRY_PASSWORD` - Password for registry.hub.sebiusz.ovh

### How to add secrets:
1. Go to repository Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Add both secrets

---

## Docker Image Structure

The Dockerfile builds a multi-project solution:
- **AirlineManager** (main web app)
- **AirlineManager.Models**
- **AirlineManager.DataAccess**
- **AirlineManager.Services**
- **AirlineManager.Middleware**

Base image: `mcr.microsoft.com/dotnet/aspnet:9.0`

Exposed ports:
- `8080` (HTTP)
- `8081` (HTTPS)

---

## Workflow Steps

### Common steps (both workflows):
1. **Checkout** - Clone the repository
2. **Setup .NET** - Install .NET 9.0 SDK
3. **Restore** - Restore NuGet packages
4. **Build** - Compile the solution
5. **Test** - Run unit tests
6. **Extract metadata** - Get version, commit hash, date
7. **Login** - Authenticate with Docker registry
8. **Build Docker image** - Create container with OCI labels
9. **Tag** - Add latest tag
10. **Inspect** - Verify image metadata
11. **Push** - Upload to registry

---

## Usage Examples

### Pull and run production image:
```bash
docker pull registry.hub.sebiusz.ovh/airlinemanager:latest
docker run -p 8080:8080 registry.hub.sebiusz.ovh/airlinemanager:latest
```

### Pull and run development image:
```bash
docker pull registry.hub.sebiusz.ovh/airlinemanager-dev:latest-dev
docker run -p 8080:8080 registry.hub.sebiusz.ovh/airlinemanager-dev:latest-dev
```

### Pull specific version:
```bash
docker pull registry.hub.sebiusz.ovh/airlinemanager:v1.2.3
docker pull registry.hub.sebiusz.ovh/airlinemanager-dev:v1.2.3-dev
```

---

## Troubleshooting

### Build fails with "Restore failed"
- Check if all project references are correct
- Verify NuGet package sources

### Docker login fails
- Verify `REGISTRY_USER` and `REGISTRY_PASSWORD` secrets
- Check registry URL: `registry.hub.sebiusz.ovh`

### Tests fail
- Check test output in GitHub Actions logs
- Run tests locally: `dotnet test`

### Image push fails
- Verify registry permissions
- Check network connectivity to registry.hub.sebiusz.ovh

---

## Maintenance

### To update .NET version:
Update the `dotnet-version` in both workflows:
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v3
  with:
    dotnet-version: '9.0.x'  # Change this
```

### To change image names:
Update the `docker build` and `docker tag` commands in both workflows.

### To add new projects:
Update `AirlineManager/Dockerfile` to copy new project files.

---

## OCI Image Metadata

Both images include comprehensive OCI labels:
- `org.opencontainers.image.title`
- `org.opencontainers.image.description`
- `org.opencontainers.image.version`
- `org.opencontainers.image.created`
- `org.opencontainers.image.revision`
- `org.opencontainers.image.source`
- `org.opencontainers.image.authors`
- `org.opencontainers.image.documentation`
- `org.opencontainers.image.url`
- `org.opencontainers.image.vendor`

View metadata:
```bash
docker inspect registry.hub.sebiusz.ovh/airlinemanager:latest --format '{{ json .Config.Labels }}' | jq .
```

---

## Contact

**Author:** sebiusz <sstachurski@gmail.com>

**Repositories:**
- Primary: https://git.sebiusz.ovh/sebiusz/AirlineManager
- Mirror: https://github.com/sebiusz76/AirlineManager

**Documentation:** https://hub.sebiusz.ovh/docs/airlinemanager

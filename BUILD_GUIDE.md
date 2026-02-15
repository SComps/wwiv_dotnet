# WWIV .NET - Build Guide

## Overview

WWIV .NET is configured for **Ahead-of-Time (AOT) compilation** and **self-contained deployment** for both Windows and Linux. This means:

- ✅ **No .NET Runtime Required** - All dependencies are bundled
- ✅ **Native Performance** - AOT compilation for maximum speed
- ✅ **Single Deployment** - Copy the publish folder and run
- ✅ **Cross-Platform** - Build for Windows or Linux from either OS

## Prerequisites

### Windows
- **.NET 9 SDK** (or later)
- **Visual Studio 2026** (optional, for IDE development)
- **PowerShell 5.1+** (for build scripts)

### Linux
- **.NET 9 SDK** (or later)
- **clang** and **zlib1g-dev** (for AOT compilation)

```bash
# Ubuntu/Debian
sudo apt-get update
sudo apt-get install -y clang zlib1g-dev

# Fedora/RHEL
sudo dnf install clang zlib-devel

# Arch
sudo pacman -S clang zlib
```

## Quick Build

### Windows

```powershell
# Build for Windows x64
.\build-windows.ps1

# Build for Windows ARM64
.\build-windows.ps1 -Architecture arm64

# Debug build
.\build-windows.ps1 -Configuration Debug
```

### Linux

```bash
# Make script executable (first time only)
chmod +x build-linux.sh

# Build for Linux x64
./build-linux.sh

# Build for Linux ARM64
./build-linux.sh Release arm64

# Debug build
./build-linux.sh Debug
```

## Manual Build Commands

### Windows (x64)

```powershell
# Clean
dotnet clean -c Release

# Restore
dotnet restore

# Build
dotnet build -c Release

# Publish AOT self-contained
dotnet publish wwiv3270\wwiv3270.vbproj `
    -c Release `
    -r win-x64 `
    --self-contained `
    -p:PublishAot=true `
    -p:PublishTrimmed=true `
    -o publish\windows-x64
```

### Linux (x64)

```bash
# Clean
dotnet clean -c Release

# Restore
dotnet restore

# Build
dotnet build -c Release

# Publish AOT self-contained
dotnet publish wwiv3270/wwiv3270.vbproj \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -p:PublishAot=true \
    -p:PublishTrimmed=true \
    -o publish/linux-x64
```

## Cross-Compilation

You can build for Linux from Windows and vice versa:

### Build Linux Binary from Windows

```powershell
dotnet publish wwiv3270\wwiv3270.vbproj `
    -c Release `
    -r linux-x64 `
    --self-contained `
    -p:PublishAot=true `
    -o publish\linux-x64
```

### Build Windows Binary from Linux

```bash
dotnet publish wwiv3270/wwiv3270.vbproj \
    -c Release \
    -r win-x64 \
    --self-contained \
    -p:PublishAot=true \
    -o publish/windows-x64
```

## Supported Platforms

| Platform | Runtime ID | Architecture |
|----------|-----------|--------------|
| Windows x64 | `win-x64` | 64-bit Intel/AMD |
| Windows ARM64 | `win-arm64` | 64-bit ARM (Surface, etc.) |
| Linux x64 | `linux-x64` | 64-bit Intel/AMD |
| Linux ARM64 | `linux-arm64` | 64-bit ARM (Raspberry Pi 4+, etc.) |
| Linux ARM | `linux-arm` | 32-bit ARM (Raspberry Pi 3, etc.) |

## Output Structure

After building, the publish folder contains:

```
publish/
├── windows-x64/
│   ├── wwiv3270.exe          # Main executable
│   ├── TN3270Framework.dll   # Framework library
│   └── [other dependencies]
│
└── linux-x64/
    ├── wwiv3270              # Main executable
    ├── TN3270Framework.dll   # Framework library
    └── [other dependencies]
```

## Binary Sizes

Typical binary sizes with AOT + Trimming:

| Platform | Approximate Size |
|----------|-----------------|
| Windows x64 | ~15-25 MB |
| Linux x64 | ~12-20 MB |
| Windows ARM64 | ~15-25 MB |
| Linux ARM64 | ~12-20 MB |

*Sizes vary based on .NET version and included features*

## Running the Binary

### Windows

```powershell
cd publish\windows-x64
.\wwiv3270.exe
```

### Linux

```bash
cd publish/linux-x64
./wwiv3270
```

**Note**: On Linux, port 23 (Telnet) requires root privileges:

```bash
# Option 1: Run as root
sudo ./wwiv3270

# Option 2: Grant capability (recommended)
sudo setcap 'cap_net_bind_service=+ep' ./wwiv3270
./wwiv3270

# Option 3: Change Telnet port to >1024 in BBS.vb
```

## Deployment

### Simple Deployment

1. Build for target platform
2. Copy entire `publish/[platform]` folder to target machine
3. Run the executable

### Systemd Service (Linux)

Create `/etc/systemd/system/wwiv3270.service`:

```ini
[Unit]
Description=WWIV BBS Server
After=network.target

[Service]
Type=simple
User=wwiv
WorkingDirectory=/opt/wwiv3270
ExecStart=/opt/wwiv3270/wwiv3270
Restart=always
RestartSec=10

# Security
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true
ReadWritePaths=/opt/wwiv3270/data

# Allow binding to port 23
AmbientCapabilities=CAP_NET_BIND_SERVICE

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable wwiv3270
sudo systemctl start wwiv3270
sudo systemctl status wwiv3270
```

### Windows Service

Use **NSSM** (Non-Sucking Service Manager):

```powershell
# Download NSSM from https://nssm.cc/
nssm install WWIV3270 "C:\wwiv3270\wwiv3270.exe"
nssm set WWIV3270 AppDirectory "C:\wwiv3270"
nssm set WWIV3270 DisplayName "WWIV BBS Server"
nssm set WWIV3270 Description "WWIV Bulletin Board System"
nssm set WWIV3270 Start SERVICE_AUTO_START
nssm start WWIV3270
```

## Troubleshooting

### AOT Compilation Fails

**Problem**: `error : AOT analysis failed`

**Solution**: Some reflection-based code may not be AOT-compatible. Check:
1. Ensure all types used with `Marshal` are properly attributed
2. Avoid dynamic type loading
3. Use `[DynamicallyAccessedMembers]` attribute where needed

### Linux: "Permission denied" on port 23

**Problem**: Cannot bind to port 23

**Solutions**:
1. Run as root: `sudo ./wwiv3270`
2. Grant capability: `sudo setcap 'cap_net_bind_service=+ep' ./wwiv3270`
3. Change port in `BBS.vb` to 2323 or higher

### Binary size too large

**Problem**: Published binary is >50MB

**Solutions**:
1. Ensure `PublishTrimmed=true` is set
2. Use `TrimMode=partial` or `full`
3. Remove unused dependencies
4. Consider `PublishSingleFile=true` for single-file deployment

### Missing dependencies on Linux

**Problem**: `error while loading shared libraries`

**Solution**: Install required libraries:

```bash
# Ubuntu/Debian
sudo apt-get install -y libicu-dev libssl-dev

# Fedora/RHEL
sudo dnf install icu openssl-libs

# Arch
sudo pacman -S icu openssl
```

## Performance Optimization

### Build for Maximum Performance

```bash
dotnet publish \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -p:PublishAot=true \
    -p:PublishTrimmed=true \
    -p:IlcOptimizationPreference=Speed \
    -p:IlcGenerateStackTraceData=false \
    -p:OptimizationPreference=Speed
```

### Build for Minimum Size

```bash
dotnet publish \
    -c Release \
    -r linux-x64 \
    --self-contained \
    -p:PublishAot=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=full \
    -p:IlcOptimizationPreference=Size \
    -p:PublishSingleFile=true
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Build WWIV

on: [push, pull_request]

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      - run: .\build-windows.ps1
      - uses: actions/upload-artifact@v3
        with:
          name: wwiv-windows-x64
          path: publish/windows-x64/

  build-linux:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      - run: sudo apt-get install -y clang zlib1g-dev
      - run: chmod +x build-linux.sh && ./build-linux.sh
      - uses: actions/upload-artifact@v3
        with:
          name: wwiv-linux-x64
          path: publish/linux-x64/
```

## Development vs Production Builds

### Development (Debug)
- Faster compilation
- Includes debug symbols
- Larger binary size
- Better error messages

```bash
dotnet build -c Debug
dotnet run --project wwiv3270
```

### Production (Release + AOT)
- Slower compilation (AOT takes time)
- Optimized for performance
- Smaller binary size
- Native code execution

```bash
./build-windows.ps1  # or ./build-linux.sh
```

## Additional Resources

- [.NET Native AOT Deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Self-Contained Deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained)
- [Runtime Identifiers](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

## Quick Reference

| Task | Command |
|------|---------|
| Build Windows | `.\build-windows.ps1` |
| Build Linux | `./build-linux.sh` |
| Run (Dev) | `dotnet run --project wwiv3270` |
| Run (Prod Windows) | `publish\windows-x64\wwiv3270.exe` |
| Run (Prod Linux) | `publish/linux-x64/wwiv3270` |
| Clean | `dotnet clean` |
| Test Build | `dotnet build` |

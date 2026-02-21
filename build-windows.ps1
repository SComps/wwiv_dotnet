# Build script for Windows (PowerShell)
# Builds self-contained AOT-compiled binaries for Windows

param(
    [string]$Configuration = "Release",
    [string]$Architecture = "x64"
)

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  WWIV .NET - Windows Build Script (AOT + Self-Contained)" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Clean previous builds
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Clean failed!" -ForegroundColor Red
    exit 1
}

# Restore dependencies
Write-Host "[2/4] Restoring dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Restore failed!" -ForegroundColor Red
    exit 1
}

# Build
Write-Host "[3/4] Building..." -ForegroundColor Yellow
dotnet build -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Publish AOT self-contained for Windows - wwiv3270
Write-Host "[4/5] Publishing AOT self-contained binary for Windows (wwiv3270)..." -ForegroundColor Yellow
$RuntimeId = "win-$Architecture"
$OutputPath = "publish\windows-$Architecture"

dotnet publish wwiv3270\wwiv3270.vbproj `
    -c $Configuration `
    -r $RuntimeId `
    --self-contained `
    -p:PublishAot=true `
    -p:PublishTrimmed=true `
    -p:PublishSingleFile=false `
    -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish wwiv3270 failed!" -ForegroundColor Red
    exit 1
}

# Publish AOT self-contained for Windows - wwivsetup
Write-Host "[5/5] Publishing AOT self-contained binary for Windows (wwivsetup)..." -ForegroundColor Yellow

dotnet publish wwivsetup\wwivsetup.vbproj `
    -c $Configuration `
    -r $RuntimeId `
    --self-contained `
    -p:PublishAot=true `
    -p:PublishTrimmed=true `
    -p:PublishSingleFile=false `
    -o $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish wwivsetup failed!" -ForegroundColor Red
    exit 1
}
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  Build Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "Output directory: $OutputPath" -ForegroundColor Cyan
Write-Host "Executable: $OutputPath\wwiv3270.exe" -ForegroundColor Cyan
Write-Host ""

# Show file size
$ExePath = Join-Path $OutputPath "wwiv3270.exe"
if (Test-Path $ExePath) {
    $FileSize = (Get-Item $ExePath).Length / 1MB
    Write-Host "Binary size: $([math]::Round($FileSize, 2)) MB" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "To run: cd $OutputPath && .\wwiv3270.exe" -ForegroundColor Yellow

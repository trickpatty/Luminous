<#
.SYNOPSIS
    Luminous - Local Development Start Script (PowerShell)

.DESCRIPTION
    Starts all local development services and applications for Luminous.

.PARAMETER Services
    Start Docker services only

.PARAMETER Api
    Start .NET API only (assumes Docker services running)

.PARAMETER Web
    Start Angular web app only

.PARAMETER Stop
    Stop all Docker services

.PARAMETER Status
    Check service status

.PARAMETER InstallCert
    Install Cosmos DB Emulator certificate

.EXAMPLE
    .\dev-start.ps1
    Start all services

.EXAMPLE
    .\dev-start.ps1 -Services
    Start only Docker services

.EXAMPLE
    .\dev-start.ps1 -Api -Web
    Start API and web app (no Docker)

.NOTES
    Prerequisites:
    - Docker Desktop with Docker Compose
    - .NET SDK 10.0
    - Node.js 20+ and npm
#>

[CmdletBinding()]
param(
    [switch]$Services,
    [switch]$Api,
    [switch]$Web,
    [switch]$Stop,
    [switch]$Status,
    [switch]$InstallCert
)

# Script configuration
$ErrorActionPreference = "Stop"
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptRoot

# =============================================================================
# Helper Functions
# =============================================================================

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "=============================================================================" -ForegroundColor Blue
    Write-Host " $Message" -ForegroundColor Blue
    Write-Host "=============================================================================" -ForegroundColor Blue
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Cyan
}

function Test-Command {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

function Test-Port {
    param([int]$Port)
    $connection = Test-NetConnection -ComputerName localhost -Port $Port -WarningAction SilentlyContinue -ErrorAction SilentlyContinue
    return $connection.TcpTestSucceeded
}

# =============================================================================
# Core Functions
# =============================================================================

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"

    $missing = $false

    # Check Docker
    if (Test-Command "docker") {
        Write-Success "Docker is installed"
    } else {
        Write-Error "Docker is not installed"
        $missing = $true
    }

    # Check Docker Compose
    try {
        $null = docker compose version 2>&1
        Write-Success "Docker Compose is installed"
    } catch {
        Write-Error "Docker Compose is not installed"
        $missing = $true
    }

    # Check .NET SDK
    if (Test-Command "dotnet") {
        $dotnetVersion = dotnet --version
        Write-Success ".NET SDK $dotnetVersion is installed"
    } else {
        Write-Error ".NET SDK is not installed"
        $missing = $true
    }

    # Check Node.js
    if (Test-Command "node") {
        $nodeVersion = node --version
        Write-Success "Node.js $nodeVersion is installed"
    } else {
        Write-Error "Node.js is not installed"
        $missing = $true
    }

    # Check npm
    if (Test-Command "npm") {
        $npmVersion = npm --version
        Write-Success "npm $npmVersion is installed"
    } else {
        Write-Error "npm is not installed"
        $missing = $true
    }

    if ($missing) {
        Write-Error "Missing prerequisites. Please install the required tools."
        exit 1
    }

    Write-Host ""
}

function Start-DockerServices {
    Write-Header "Starting Docker Services"

    Push-Location $ProjectRoot

    try {
        $running = docker compose ps --quiet 2>$null
        if ($running) {
            Write-Warning "Docker services are already running"
            Write-Info "Use 'docker compose restart' to restart services"
        } else {
            Write-Info "Starting Docker Compose services..."
            docker compose up -d

            Write-Info "Waiting for services to be ready..."
            Start-Sleep -Seconds 5

            # Check CosmosDB
            Write-Info "Waiting for CosmosDB Emulator (this may take 2-3 minutes)..."
            $maxAttempts = 30
            $attempt = 1
            while ($attempt -le $maxAttempts) {
                try {
                    $null = Invoke-WebRequest -Uri "https://localhost:8081/_explorer/emulator.pem" -SkipCertificateCheck -TimeoutSec 5 -ErrorAction SilentlyContinue
                    Write-Success "CosmosDB Emulator is ready"
                    break
                } catch {
                    if ($attempt -eq $maxAttempts) {
                        Write-Warning "CosmosDB Emulator may still be starting"
                    }
                    Start-Sleep -Seconds 5
                    $attempt++
                }
            }

            # Check Redis
            try {
                $pong = docker exec luminous-redis redis-cli ping 2>$null
                if ($pong -eq "PONG") {
                    Write-Success "Redis is ready"
                }
            } catch {
                Write-Warning "Redis may still be starting"
            }

            Write-Success "Azurite is running"
        }

        Write-Host ""
        Write-Success "Docker services started"
        Write-Host ""
        Write-Host "Service endpoints:"
        Write-Host "  CosmosDB:    https://localhost:8081"
        Write-Host "  Azurite:     http://localhost:10000 (Blob)"
        Write-Host "  Redis:       localhost:6379"
        Write-Host "  MailHog:     http://localhost:8025 (Web UI)"
        Write-Host ""
    } finally {
        Pop-Location
    }
}

function Stop-DockerServices {
    Write-Header "Stopping Docker Services"

    Push-Location $ProjectRoot

    try {
        docker compose down
        Write-Success "Docker services stopped"
    } finally {
        Pop-Location
    }
}

function Start-Api {
    Write-Header "Starting .NET API"

    $apiPath = Join-Path $ProjectRoot "src\Luminous.Api"
    Push-Location $apiPath

    try {
        # Restore packages if needed
        if (-not (Test-Path (Join-Path $apiPath "bin"))) {
            Write-Info "Restoring NuGet packages..."
            dotnet restore
        }

        Write-Info "Starting API with hot reload..."
        Write-Info "API will be available at http://localhost:5000"
        Write-Info "Swagger UI: http://localhost:5000/swagger"
        Write-Host ""

        dotnet watch run --no-hot-reload
    } finally {
        Pop-Location
    }
}

function Start-Web {
    Write-Header "Starting Angular Web App"

    $webPath = Join-Path $ProjectRoot "clients\web"
    Push-Location $webPath

    try {
        # Install dependencies if needed
        if (-not (Test-Path (Join-Path $webPath "node_modules"))) {
            Write-Info "Installing npm dependencies..."
            npm install
        }

        Write-Info "Starting Angular development server..."
        Write-Info "Web app will be available at http://localhost:4200"
        Write-Host ""

        npm start
    } finally {
        Pop-Location
    }
}

function Show-Status {
    Write-Header "Service Status"

    Push-Location $ProjectRoot

    try {
        Write-Host "Docker Services:"
        docker compose ps 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  Docker Compose not running"
        }

        Write-Host ""
        Write-Host "Port Usage:"

        $ports = @{
            5000 = "API"
            4200 = "Web"
            8081 = "Cosmos"
            6379 = "Redis"
            10000 = "Blob"
        }

        foreach ($port in $ports.Keys) {
            $status = if (Test-Port $port) { "IN USE" } else { "FREE" }
            Write-Host "  Port $port ($($ports[$port])): $status"
        }

        Write-Host ""
    } finally {
        Pop-Location
    }
}

function Install-CosmosCert {
    Write-Header "Installing Cosmos DB Emulator Certificate"

    Write-Info "Downloading certificate from emulator..."

    $certPath = Join-Path $env:TEMP "cosmos-emulator.crt"

    try {
        Invoke-WebRequest -Uri "https://localhost:8081/_explorer/emulator.pem" -SkipCertificateCheck -OutFile $certPath
        Write-Success "Certificate downloaded to $certPath"

        Write-Info "Importing certificate to Trusted Root store..."
        Write-Info "You may be prompted for administrator privileges..."

        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($certPath)
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("Root", "CurrentUser")
        $store.Open("ReadWrite")
        $store.Add($cert)
        $store.Close()

        Write-Success "Certificate installed successfully"
    } catch {
        Write-Error "Failed to install certificate: $_"
        Write-Info "Make sure the Cosmos DB Emulator is running first."
    }
}

# =============================================================================
# Main
# =============================================================================

Write-Header "Luminous Local Development"
Write-Host "Project Root: $ProjectRoot"

# Handle specific actions
if ($Stop) {
    Stop-DockerServices
    exit 0
}

if ($Status) {
    Show-Status
    exit 0
}

if ($InstallCert) {
    Install-CosmosCert
    exit 0
}

Test-Prerequisites

# Determine what to start
$startAll = -not ($Services -or $Api -or $Web)

if ($startAll) {
    Start-DockerServices

    Write-Info "Starting API and Web app in separate terminals..."
    Write-Info "Open two new PowerShell windows and run:"
    Write-Host ""
    Write-Host "  Terminal 1 (API):  cd $ProjectRoot; .\scripts\dev-start.ps1 -Api"
    Write-Host "  Terminal 2 (Web):  cd $ProjectRoot; .\scripts\dev-start.ps1 -Web"
    Write-Host ""
    Write-Info "Or use the provided VS Code launch configurations"
    exit 0
}

if ($Services) {
    Start-DockerServices
}

if ($Api) {
    Start-Api
}

if ($Web) {
    Start-Web
}

Write-Success "Development environment ready!"

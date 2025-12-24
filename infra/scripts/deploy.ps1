<#
.SYNOPSIS
    Luminous - Azure Infrastructure Deployment Script (PowerShell)

.DESCRIPTION
    Deploys Luminous Azure infrastructure using Bicep templates.

    Prerequisites:
    - Azure CLI installed and logged in (az login)
    - Correct subscription selected (az account set -s <subscription>)
    - Resource group must exist before deployment. Create with:
      az group create --name rg-lum-<env> --location eastus2

.PARAMETER Environment
    The target environment (dev, stg, prd)

.PARAMETER WhatIf
    Preview changes without deploying

.PARAMETER Location
    Azure region for deployment (default: eastus2)

.EXAMPLE
    .\deploy.ps1 -Environment dev

.EXAMPLE
    .\deploy.ps1 -Environment stg -WhatIf

.EXAMPLE
    .\deploy.ps1 -Environment prd -Location westus2
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet('dev', 'stg', 'prd')]
    [string]$Environment,

    [Parameter()]
    [switch]$WhatIf,

    [Parameter()]
    [string]$Location = 'eastus2'
)

# Script variables
$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BicepDir = Join-Path $ScriptDir '..\bicep'
$RgPrefix = 'rg-lum'
$DeploymentName = "luminous-infra-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# =============================================================================
# Functions
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
    Write-Host "ℹ $Message" -ForegroundColor Blue
}

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"

    # Check Azure CLI
    try {
        $azVersion = az version 2>$null | ConvertFrom-Json
        Write-Success "Azure CLI is installed (v$($azVersion.'azure-cli'))"
    }
    catch {
        Write-Error "Azure CLI is not installed. Please install it first."
        Write-Host "  https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    }

    # Check if logged in
    try {
        $account = az account show 2>$null | ConvertFrom-Json
        Write-Success "Logged in to Azure"
        Write-Info "Current subscription: $($account.name)"
    }
    catch {
        Write-Error "Not logged in to Azure. Please run 'az login' first."
        exit 1
    }

    # Check Bicep
    try {
        $bicepVersion = az bicep version 2>$null
        Write-Success "Bicep CLI is available"
    }
    catch {
        Write-Warning "Bicep CLI not found. Installing..."
        az bicep install
        Write-Success "Bicep CLI installed"
    }
}

function Test-BicepTemplates {
    Write-Header "Validating Bicep Templates"

    $mainBicep = Join-Path $BicepDir 'main.bicep'

    Write-Info "Building main.bicep..."

    try {
        $result = az bicep build --file $mainBicep --stdout 2>&1
        Write-Success "Bicep templates are valid"
    }
    catch {
        Write-Error "Bicep validation failed"
        az bicep build --file $mainBicep
        exit 1
    }
}

function Deploy-Infrastructure {
    param(
        [string]$Env,
        [bool]$IsWhatIf
    )

    Write-Header "Deploying to $Env Environment"

    $paramFile = Join-Path $BicepDir "parameters\$Env.bicepparam"
    $mainBicep = Join-Path $BicepDir 'main.bicep'
    $resourceGroup = "$RgPrefix-$Env"

    # Check parameter file exists
    if (-not (Test-Path $paramFile)) {
        Write-Error "Parameter file not found: $paramFile"
        exit 1
    }
    Write-Success "Using parameter file: $paramFile"

    # Check resource group exists
    $rgExists = az group show --name $resourceGroup 2>$null
    if (-not $rgExists) {
        Write-Error "Resource group '$resourceGroup' does not exist."
        Write-Info "Create it with: az group create --name $resourceGroup --location $Location"
        exit 1
    }
    Write-Success "Resource group exists: $resourceGroup"

    Write-Info "Starting deployment..."
    Write-Host ""

    try {
        if ($IsWhatIf) {
            Write-Info "Running what-if analysis..."
            az deployment group what-if `
                --resource-group $resourceGroup `
                --template-file $mainBicep `
                --parameters "@$paramFile"

            Write-Success "What-if analysis completed"
        }
        else {
            az deployment group create `
                --name $DeploymentName `
                --resource-group $resourceGroup `
                --template-file $mainBicep `
                --parameters "@$paramFile"

            Write-Success "Deployment completed successfully"
            Write-Info "Deployment name: $DeploymentName"
        }
    }
    catch {
        Write-Error "Deployment failed: $_"
        exit 1
    }
}

function Show-Outputs {
    param(
        [string]$Env
    )

    Write-Header "Deployment Outputs"

    $resourceGroup = "$RgPrefix-$Env"

    try {
        az deployment group show `
            --name $DeploymentName `
            --resource-group $resourceGroup `
            --query properties.outputs `
            -o table
    }
    catch {
        Write-Warning "Could not retrieve outputs"
    }
}

# =============================================================================
# Main
# =============================================================================

Write-Header "Luminous Infrastructure Deployment"
Write-Host "Environment: $Environment"
Write-Host "Location:    $Location"
Write-Host "What-If:     $WhatIf"

Test-Prerequisites
Test-BicepTemplates
Deploy-Infrastructure -Env $Environment -IsWhatIf $WhatIf

if (-not $WhatIf) {
    Show-Outputs -Env $Environment
}

Write-Header "Deployment Complete"
Write-Success "Infrastructure deployment finished for $Environment environment"

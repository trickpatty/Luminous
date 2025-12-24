#!/bin/bash
# =============================================================================
# Luminous - Azure Infrastructure Deployment Script
# =============================================================================
# Deploys Luminous Azure infrastructure using Bicep templates.
#
# Usage:
#   ./deploy.sh <environment> [--what-if]
#
# Examples:
#   ./deploy.sh dev           # Deploy to development
#   ./deploy.sh stg           # Deploy to staging
#   ./deploy.sh prd           # Deploy to production
#   ./deploy.sh dev --what-if # Preview changes without deploying
#
# Prerequisites:
#   - Azure CLI installed and logged in (az login)
#   - Correct subscription selected (az account set -s <subscription>)
#   - Resource group must exist before deployment. Create with:
#     az group create --name rg-lum-<env> --location eastus2
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BICEP_DIR="$SCRIPT_DIR/../bicep"

# Default values
LOCATION="eastus2"
RG_PREFIX="rg-lum"
DEPLOYMENT_NAME="luminous-infra-$(date +%Y%m%d-%H%M%S)"

# =============================================================================
# Functions
# =============================================================================

print_header() {
    echo ""
    echo -e "${BLUE}=============================================================================${NC}"
    echo -e "${BLUE} $1${NC}"
    echo -e "${BLUE}=============================================================================${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

usage() {
    echo "Usage: $0 <environment> [options]"
    echo ""
    echo "Environments:"
    echo "  dev       Deploy to development environment"
    echo "  stg       Deploy to staging environment"
    echo "  prd       Deploy to production environment"
    echo ""
    echo "Options:"
    echo "  --what-if     Preview changes without deploying"
    echo "  --location    Azure region (default: eastus2)"
    echo "  --help        Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 dev"
    echo "  $0 stg --what-if"
    echo "  $0 prd --location westus2"
    exit 1
}

check_prerequisites() {
    print_header "Checking Prerequisites"

    # Check Azure CLI
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it first."
        echo "  https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi
    print_success "Azure CLI is installed"

    # Check if logged in
    if ! az account show &> /dev/null; then
        print_error "Not logged in to Azure. Please run 'az login' first."
        exit 1
    fi
    print_success "Logged in to Azure"

    # Check Bicep
    if ! az bicep version &> /dev/null; then
        print_warning "Bicep CLI not found. Installing..."
        az bicep install
    fi
    print_success "Bicep CLI is available"

    # Show current subscription
    SUBSCRIPTION=$(az account show --query name -o tsv)
    print_info "Current subscription: $SUBSCRIPTION"
}

validate_environment() {
    case $1 in
        dev|stg|prd)
            return 0
            ;;
        *)
            print_error "Invalid environment: $1"
            usage
            ;;
    esac
}

validate_bicep() {
    print_header "Validating Bicep Templates"

    print_info "Building main.bicep..."
    if az bicep build --file "$BICEP_DIR/main.bicep" --stdout > /dev/null 2>&1; then
        print_success "Bicep templates are valid"
    else
        print_error "Bicep validation failed"
        az bicep build --file "$BICEP_DIR/main.bicep"
        exit 1
    fi
}

deploy_infrastructure() {
    local env=$1
    local what_if=$2
    local param_file="$BICEP_DIR/parameters/${env}.bicepparam"
    local resource_group="${RG_PREFIX}-${env}"

    print_header "Deploying to $env Environment"

    # Check parameter file exists
    if [[ ! -f "$param_file" ]]; then
        print_error "Parameter file not found: $param_file"
        exit 1
    fi
    print_success "Using parameter file: $param_file"

    # Check resource group exists
    if ! az group show --name "$resource_group" &> /dev/null; then
        print_error "Resource group '$resource_group' does not exist."
        print_info "Create it with: az group create --name $resource_group --location $LOCATION"
        exit 1
    fi
    print_success "Resource group exists: $resource_group"

    # Build deployment command (resource group scope)
    local deploy_cmd="az deployment group create \
        --name $DEPLOYMENT_NAME \
        --resource-group $resource_group \
        --template-file $BICEP_DIR/main.bicep \
        --parameters @$param_file"

    if [[ "$what_if" == "true" ]]; then
        print_info "Running what-if analysis..."
        deploy_cmd="az deployment group what-if \
            --resource-group $resource_group \
            --template-file $BICEP_DIR/main.bicep \
            --parameters @$param_file"
    fi

    print_info "Starting deployment..."
    echo ""

    if eval $deploy_cmd; then
        if [[ "$what_if" == "true" ]]; then
            print_success "What-if analysis completed"
        else
            print_success "Deployment completed successfully"
            print_info "Deployment name: $DEPLOYMENT_NAME"
        fi
    else
        print_error "Deployment failed"
        exit 1
    fi
}

show_outputs() {
    local env=$1
    local resource_group="${RG_PREFIX}-${env}"

    print_header "Deployment Outputs"

    az deployment group show \
        --name $DEPLOYMENT_NAME \
        --resource-group $resource_group \
        --query properties.outputs \
        -o table 2>/dev/null || print_warning "Could not retrieve outputs"
}

# =============================================================================
# Main
# =============================================================================

# Parse arguments
ENVIRONMENT=""
WHAT_IF="false"

while [[ $# -gt 0 ]]; do
    case $1 in
        dev|stg|prd)
            ENVIRONMENT=$1
            shift
            ;;
        --what-if)
            WHAT_IF="true"
            shift
            ;;
        --location)
            LOCATION=$2
            shift 2
            ;;
        --help|-h)
            usage
            ;;
        *)
            print_error "Unknown option: $1"
            usage
            ;;
    esac
done

# Validate environment was provided
if [[ -z "$ENVIRONMENT" ]]; then
    print_error "Environment is required"
    usage
fi

# Run deployment
print_header "Luminous Infrastructure Deployment"
echo "Environment: $ENVIRONMENT"
echo "Location:    $LOCATION"
echo "What-If:     $WHAT_IF"

validate_environment "$ENVIRONMENT"
check_prerequisites
validate_bicep
deploy_infrastructure "$ENVIRONMENT" "$WHAT_IF"

if [[ "$WHAT_IF" == "false" ]]; then
    show_outputs "$ENVIRONMENT"
fi

print_header "Deployment Complete"
print_success "Infrastructure deployment finished for $ENVIRONMENT environment"

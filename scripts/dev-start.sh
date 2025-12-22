#!/bin/bash
# =============================================================================
# Luminous - Local Development Start Script
# =============================================================================
# Starts all local development services and applications.
#
# Usage:
#   ./dev-start.sh              # Start all services
#   ./dev-start.sh --api        # Start API only (assumes Docker services running)
#   ./dev-start.sh --web        # Start Web app only
#   ./dev-start.sh --services   # Start Docker services only
#   ./dev-start.sh --stop       # Stop all services
#   ./dev-start.sh --status     # Check service status
#
# Prerequisites:
#   - Docker and Docker Compose installed
#   - .NET SDK 10.0 installed
#   - Node.js 20+ and npm installed
# =============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Script directory (resolve to project root)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

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
    echo -e "${CYAN}ℹ $1${NC}"
}

usage() {
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  --all       Start all services (default)"
    echo "  --services  Start Docker services only"
    echo "  --api       Start .NET API only (assumes Docker services running)"
    echo "  --web       Start Angular web app only"
    echo "  --stop      Stop all services"
    echo "  --status    Check service status"
    echo "  --help      Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                  # Start everything"
    echo "  $0 --services       # Start only Docker services"
    echo "  $0 --api --web      # Start API and web app (no Docker)"
    exit 0
}

check_prerequisites() {
    print_header "Checking Prerequisites"

    local missing=0

    # Check Docker
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed"
        missing=1
    else
        print_success "Docker is installed"
    fi

    # Check Docker Compose
    if ! command -v docker &> /dev/null || ! docker compose version &> /dev/null; then
        if ! command -v docker-compose &> /dev/null; then
            print_error "Docker Compose is not installed"
            missing=1
        else
            print_success "Docker Compose (legacy) is installed"
        fi
    else
        print_success "Docker Compose is installed"
    fi

    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed"
        missing=1
    else
        local dotnet_version=$(dotnet --version)
        print_success ".NET SDK $dotnet_version is installed"
    fi

    # Check Node.js
    if ! command -v node &> /dev/null; then
        print_error "Node.js is not installed"
        missing=1
    else
        local node_version=$(node --version)
        print_success "Node.js $node_version is installed"
    fi

    # Check npm
    if ! command -v npm &> /dev/null; then
        print_error "npm is not installed"
        missing=1
    else
        local npm_version=$(npm --version)
        print_success "npm $npm_version is installed"
    fi

    if [ $missing -eq 1 ]; then
        print_error "Missing prerequisites. Please install the required tools."
        exit 1
    fi

    echo ""
}

start_docker_services() {
    print_header "Starting Docker Services"

    cd "$PROJECT_ROOT"

    if docker compose ps --quiet 2>/dev/null | grep -q .; then
        print_warning "Docker services are already running"
        print_info "Use 'docker compose restart' to restart services"
    else
        print_info "Starting Docker Compose services..."
        docker compose up -d

        print_info "Waiting for services to be ready..."
        sleep 5

        # Check CosmosDB
        local max_attempts=30
        local attempt=1
        while [ $attempt -le $max_attempts ]; do
            if curl -fks https://localhost:8081/_explorer/emulator.pem > /dev/null 2>&1; then
                print_success "CosmosDB Emulator is ready"
                break
            fi
            if [ $attempt -eq $max_attempts ]; then
                print_warning "CosmosDB Emulator may still be starting (can take 2-3 minutes)"
            fi
            sleep 5
            attempt=$((attempt + 1))
        done

        # Check Redis
        if docker exec luminous-redis redis-cli ping 2>/dev/null | grep -q PONG; then
            print_success "Redis is ready"
        else
            print_warning "Redis may still be starting"
        fi

        # Check Azurite
        if curl -s http://localhost:10000/devstoreaccount1 > /dev/null 2>&1; then
            print_success "Azurite is ready"
        else
            print_success "Azurite is running"
        fi
    fi

    echo ""
    print_success "Docker services started"
    echo ""
    echo "Service endpoints:"
    echo "  CosmosDB:    https://localhost:8081"
    echo "  Azurite:     http://localhost:10000 (Blob)"
    echo "  Redis:       localhost:6379"
    echo "  MailHog:     http://localhost:8025 (Web UI)"
    echo ""
}

stop_docker_services() {
    print_header "Stopping Docker Services"

    cd "$PROJECT_ROOT"

    docker compose down
    print_success "Docker services stopped"
}

start_api() {
    print_header "Starting .NET API"

    cd "$PROJECT_ROOT/src/Luminous.Api"

    # Restore packages if needed
    if [ ! -d "$PROJECT_ROOT/src/Luminous.Api/bin" ]; then
        print_info "Restoring NuGet packages..."
        dotnet restore
    fi

    print_info "Starting API with hot reload..."
    print_info "API will be available at http://localhost:5000"
    print_info "Swagger UI: http://localhost:5000/swagger"
    echo ""

    dotnet watch run --no-hot-reload
}

start_web() {
    print_header "Starting Angular Web App"

    cd "$PROJECT_ROOT/clients/web"

    # Install dependencies if needed
    if [ ! -d "node_modules" ]; then
        print_info "Installing npm dependencies..."
        npm install
    fi

    print_info "Starting Angular development server..."
    print_info "Web app will be available at http://localhost:4200"
    echo ""

    npm start
}

show_status() {
    print_header "Service Status"

    cd "$PROJECT_ROOT"

    echo "Docker Services:"
    docker compose ps 2>/dev/null || echo "  Docker Compose not running"

    echo ""
    echo "Port Usage:"
    echo "  Port 5000 (API):     $(lsof -i :5000 2>/dev/null | grep LISTEN > /dev/null && echo "IN USE" || echo "FREE")"
    echo "  Port 4200 (Web):     $(lsof -i :4200 2>/dev/null | grep LISTEN > /dev/null && echo "IN USE" || echo "FREE")"
    echo "  Port 8081 (Cosmos):  $(lsof -i :8081 2>/dev/null | grep LISTEN > /dev/null && echo "IN USE" || echo "FREE")"
    echo "  Port 6379 (Redis):   $(lsof -i :6379 2>/dev/null | grep LISTEN > /dev/null && echo "IN USE" || echo "FREE")"
    echo "  Port 10000 (Blob):   $(lsof -i :10000 2>/dev/null | grep LISTEN > /dev/null && echo "IN USE" || echo "FREE")"
    echo ""
}

install_cosmos_cert() {
    print_header "Installing Cosmos DB Emulator Certificate"

    print_info "Downloading certificate from emulator..."

    local cert_path="/tmp/cosmos-emulator.crt"

    # Download the certificate
    curl -k https://localhost:8081/_explorer/emulator.pem > "$cert_path" 2>/dev/null

    if [ -f "$cert_path" ]; then
        print_success "Certificate downloaded"

        # Check if running on macOS
        if [[ "$OSTYPE" == "darwin"* ]]; then
            print_info "On macOS, run:"
            echo "  sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain $cert_path"
        elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
            print_info "On Linux (Ubuntu/Debian), run:"
            echo "  sudo cp $cert_path /usr/local/share/ca-certificates/cosmos-emulator.crt"
            echo "  sudo update-ca-certificates"
        fi
    else
        print_error "Failed to download certificate. Is the emulator running?"
    fi
}

# =============================================================================
# Main
# =============================================================================

# Parse arguments
START_ALL=true
START_SERVICES=false
START_API=false
START_WEB=false
STOP=false
STATUS=false

if [ $# -eq 0 ]; then
    START_ALL=true
else
    START_ALL=false
    while [[ $# -gt 0 ]]; do
        case $1 in
            --all)
                START_ALL=true
                shift
                ;;
            --services)
                START_SERVICES=true
                shift
                ;;
            --api)
                START_API=true
                shift
                ;;
            --web)
                START_WEB=true
                shift
                ;;
            --stop)
                STOP=true
                shift
                ;;
            --status)
                STATUS=true
                shift
                ;;
            --install-cert)
                check_prerequisites
                install_cosmos_cert
                exit 0
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
fi

# Execute based on arguments
print_header "Luminous Local Development"
echo "Project Root: $PROJECT_ROOT"

if [ "$STOP" = true ]; then
    stop_docker_services
    exit 0
fi

if [ "$STATUS" = true ]; then
    show_status
    exit 0
fi

check_prerequisites

if [ "$START_ALL" = true ]; then
    start_docker_services

    print_info "Starting API and Web app in separate terminals..."
    print_info "Open two new terminal windows and run:"
    echo ""
    echo "  Terminal 1 (API):  cd $PROJECT_ROOT && ./scripts/dev-start.sh --api"
    echo "  Terminal 2 (Web):  cd $PROJECT_ROOT && ./scripts/dev-start.sh --web"
    echo ""
    print_info "Or use the provided VS Code launch configurations"
    exit 0
fi

if [ "$START_SERVICES" = true ]; then
    start_docker_services
fi

if [ "$START_API" = true ]; then
    start_api
fi

if [ "$START_WEB" = true ]; then
    start_web
fi

print_success "Development environment ready!"

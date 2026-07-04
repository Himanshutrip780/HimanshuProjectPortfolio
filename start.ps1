# Self-Hosted Portfolio Platform Startup Script
# Operating System: Windows PowerShell

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   Starting Self-Hosted Portfolio Platform                " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Verify Docker Desktop is running
Write-Host "[1/4] Verifying Docker Desktop is running..." -ForegroundColor Yellow
docker info > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Docker Desktop is not running! Please start Docker Desktop first and try again." -ForegroundColor Red
    Exit 1
}
Write-Host "√ Docker is running." -ForegroundColor Green

# 2. Check if .env file exists
Write-Host "[2/4] Verifying environment configuration..." -ForegroundColor Yellow
$envPath = Join-Path $PSScriptRoot ".env"

if (-Not (Test-Path $envPath)) {
    Write-Host "[WARNING] .env file not found! Recreating default .env..." -ForegroundColor Magenta
    Set-Content -Path $envPath -Value @"
# Workflow.io Environment Configuration
POSTGRES_PASSWORD=postgres
JWT_SECURITY_KEY=THIS_IS_SUPER_SECRET_KEY_123456789_WORKFLOW_IO
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest

# HireNow Environment Configuration
MSSQL_SA_PASSWORD=Password_123_Strong!
"@
}

# Verify Named Tunnel credentials file and origin certificate exist in user profile
$credsPath = Join-Path $env:USERPROFILE ".cloudflared\50a585f5-a020-4e2c-8c66-a82b2594f3f3.json"
$certPath = Join-Path $env:USERPROFILE ".cloudflared\cert.pem"

if (-Not (Test-Path $credsPath)) {
    Write-Host "[ERROR] Cloudflare credentials file not found at: $credsPath" -ForegroundColor Red
    Write-Host "Please ensure you have created the named tunnel 'portfolio' using:" -ForegroundColor Yellow
    Write-Host "   cloudflared tunnel create portfolio" -ForegroundColor Yellow
    Exit 1
}

if (-Not (Test-Path $certPath)) {
    Write-Host "[ERROR] Cloudflare origin certificate not found at: $certPath" -ForegroundColor Red
    Write-Host "Please ensure you have authenticated cloudflared using:" -ForegroundColor Yellow
    Write-Host "   cloudflared tunnel login" -ForegroundColor Yellow
    Exit 1
}

Write-Host "√ Cloudflare Named Tunnel credentials file and origin certificate found." -ForegroundColor Green

# 3. Boot services using Docker Compose
Write-Host ""
Write-Host "[3/4] Building and launching containers in the background..." -ForegroundColor Yellow
docker compose up -d --build

if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Failed to start containers. Please review Docker logs above." -ForegroundColor Red
    Exit 1
}

# 4. Monitor startup and wait for Nginx proxy to be healthy
Write-Host ""
Write-Host "[4/4] Waiting for services to initialize healthchecks..." -ForegroundColor Yellow
Write-Host "This may take 1-2 minutes on first startup while databases initialize..." -ForegroundColor DarkGray

$timeout = 120 # seconds
$interval = 5  # seconds
$elapsed = 0
$isHealthy = $false

while ($elapsed -lt $timeout) {
    $nginxStatus = docker inspect --format='{{json .State.Health.Status}}' nginx-proxy 2>$null
    
    if ($nginxStatus -eq '"healthy"') {
        $isHealthy = $true
        break
    }
    
    Start-Sleep -Seconds $interval
    $elapsed += $interval
    Write-Host "Waiting... ($elapsed/$timeout seconds elapsed)" -ForegroundColor DarkGray
}

Write-Host ""
if ($isHealthy) {
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host "   SUCCESS: All Services Deployed and Healthy!            " -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host "You can access your applications at:" -ForegroundColor White
    Write-Host " - Portfolio:  https://himanshuprojectportfolio.xyz" -ForegroundColor Cyan
    Write-Host " - HireNow:    https://hirenow.himanshuprojectportfolio.xyz" -ForegroundColor Cyan
    Write-Host " - Workflow:   https://workflow.himanshuprojectportfolio.xyz" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Local testing addresses (bypassing DNS/Tunnel):" -ForegroundColor DarkGray
    Write-Host " - Add mapping to C:\Windows\System32\drivers\etc\hosts:" -ForegroundColor DarkGray
    Write-Host "   127.0.0.1 himanshuprojectportfolio.xyz hirenow.himanshuprojectportfolio.xyz workflow.himanshuprojectportfolio.xyz" -ForegroundColor DarkGray
    Write-Host " - Access locally via: http://himanshuprojectportfolio.xyz" -ForegroundColor DarkGray
} else {
    Write-Host "==========================================================" -ForegroundColor Red
    Write-Host "   WARNING: Startup Timeout Exceeded                      " -ForegroundColor Red
    Write-Host "==========================================================" -ForegroundColor Red
    Write-Host "Nginx proxy did not report healthy. Some containers might still be starting."
    Write-Host "Check container health status manually with:" -ForegroundColor Yellow
    Write-Host "   docker compose ps" -ForegroundColor Yellow
}
Write-Host ""

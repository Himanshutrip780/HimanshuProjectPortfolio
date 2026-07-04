# Windows Reboot Recovery and Autostart Setup Script
# Operating System: Windows PowerShell (Requires Administrator Permissions)

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   Configuring Windows Reboot Autostart Persistence       " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Check for Administrator permissions
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-Not $isAdmin) {
    Write-Host "[WARNING] This script is not running as Administrator! Some persistent settings may fail." -ForegroundColor Yellow
    Write-Host "Please close this window and run PowerShell as Administrator if errors occur." -ForegroundColor DarkGray
    Write-Host ""
}

# 1. Enable Docker Desktop Autostart
Write-Host "[1/3] Configuring Docker Desktop to start on login..." -ForegroundColor Yellow
$dockerRegistryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
$dockerValueName = "Docker Desktop"
$dockerExePath = "$env:ProgramFiles\Docker\Docker\Docker Desktop.exe"

if (Test-Path $dockerExePath) {
    Set-ItemProperty -Path $dockerRegistryPath -Name $dockerValueName -Value "`"$dockerExePath`" -autostart" -ErrorAction SilentlyContinue
    Write-Host "√ Configured Docker Desktop registry run key." -ForegroundColor Green
} else {
    Write-Host "[INFO] Docker Desktop executable path not found at default location. Please ensure 'Start Docker Desktop when you log in' is checked in Docker Desktop -> Settings -> General." -ForegroundColor Cyan
}

# 2. Configure Task Scheduler Job (Fallback for System Boot before Login)
# This will start the Docker Compose stack as a system service.
Write-Host ""
Write-Host "[2/3] Registering Task Scheduler autostart backup job..." -ForegroundColor Yellow
$taskName = "AutostartPortfolioPlatform"
$composeFolder = $PSScriptRoot
$actionScript = @"
cd "$composeFolder"
docker compose up -d
"@

$actionScriptPath = Join-Path $composeFolder "autostart-task.ps1"
Set-Content -Path $actionScriptPath -Value $actionScript
Write-Host "Created autostart task script: $actionScriptPath" -ForegroundColor DarkGray

# Define task parameters
$trigger = New-ScheduledTaskTrigger -AtStartup
$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -WindowStyle Hidden -File `"$actionScriptPath`""
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

# Register task (Requires Administrator)
if ($isAdmin) {
    try {
        # Unregister task if already exists
        Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue
        Register-ScheduledTask -TaskName $taskName -Trigger $trigger -Action $action -Settings $settings -User "NT AUTHORITY\SYSTEM" -RunLevel Highest -ErrorAction Stop
        Write-Host "√ Registered Task Scheduler job '$taskName' to run at system startup." -ForegroundColor Green
    } catch {
        Write-Host "[ERROR] Failed to register Scheduled Task: $_" -ForegroundColor Red
    }
} else {
    Write-Host "[SKIP] Scheduled Task registration skipped due to lack of Administrator privileges." -ForegroundColor Yellow
}

# 3. Verify Docker restart policy
Write-Host ""
Write-Host "[3/3] Verifying Docker Compose container restart policies..." -ForegroundColor Yellow
$composePath = Join-Path $composeFolder "docker-compose.yml"
if (Test-Path $composePath) {
    $composeContent = Get-Content $composePath
    $restartsCount = ($composeContent | Select-String "restart: unless-stopped").Count
    Write-Host "√ Found $restartsCount containers configured with 'restart: unless-stopped'." -ForegroundColor Green
    Write-Host "   This ensures Docker automatically boots them when the engine starts." -ForegroundColor DarkGray
} else {
    Write-Host "[ERROR] docker-compose.yml not found in current folder!" -ForegroundColor Red
}

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "   Persistence configuration completed!                   " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "Your PC will now automatically restore all web services, "
Write-Host "databases, proxy, and tunnel after a system crash or reboot."
Write-Host ""

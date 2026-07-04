$ErrorActionPreference = "Stop"

$resourceGroup = "rg-workflowio-prod"
$acrServer = "acrworkflowio.azurecr.io"
$acrName = "acrworkflowio"
$envId = "/subscriptions/e04ee03c-12b1-4d19-b814-38b2c17646ba/resourceGroups/rg-workflowio-prod/providers/Microsoft.App/managedEnvironments/cae-workflowio-prod"

# The standard environment variables needed for all APIs
$envVarsArray = @(
    "ConnectionStrings__DefaultConnection=Host=psql-workflowio-db.postgres.database.azure.com;Database=workflow_io;Username=postgresAdmin;Password=P@ssw0rd1234!;Ssl Mode=Require;Trust Server Certificate=true;",
    "Jwt__SecurityKey=YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Jwt__Issuer=https://workflow.io.local",
    "Jwt__Audience=workflow.io-api",
    "Database__ApplyMigrationsOnStartup=true"
)

# Define the remaining microservices
$services = @(
    @{ Name = "projectapi"; Folder = "ProjectApi"; ClusterId = "projects" },
    @{ Name = "taskapi"; Folder = "TaskApi"; ClusterId = "tasks" },
    @{ Name = "commentapi"; Folder = "CommentApi"; ClusterId = "comments" },
    @{ Name = "fileapi"; Folder = "FileApi"; ClusterId = "files" },
    @{ Name = "notificationapi"; Folder = "NotificationApi"; ClusterId = "notifications" },
    @{ Name = "activityapi"; Folder = "ActivityApi"; ClusterId = "activities" },
    @{ Name = "analyticsapi"; Folder = "AnalyticsApi"; ClusterId = "analytics" },
    @{ Name = "realtimeapi"; Folder = "RealtimeApi"; ClusterId = "realtime" }
)

$fqdns = @{}

Write-Host "Starting deployment of remaining APIs..." -ForegroundColor Cyan

foreach ($svc in $services) {
    Write-Host "`n=================================================" -ForegroundColor Magenta
    Write-Host "Deploying $($svc.Name)..." -ForegroundColor Yellow
    Write-Host "=================================================" -ForegroundColor Magenta
    
    # 1. Build and push image to ACR
    Write-Host "Building Docker image for $($svc.Name)..."
    az acr build --registry $acrName --image "$($svc.Name):latest" "./$($svc.Folder)"
    
    # 2. Check if container app already exists
    $exists = az containerapp show -n $svc.Name -g $resourceGroup --query id -o tsv 2>$null
    
    if (-not $exists) {
        Write-Host "Creating brand new Container App for $($svc.Name)..." -ForegroundColor Green
        az containerapp create `
            --name $svc.Name `
            --resource-group $resourceGroup `
            --environment $envId `
            --image "$acrServer/$($svc.Name):latest" `
            --target-port 8080 `
            --ingress internal `
            --min-replicas 1 `
            --max-replicas 10 `
            --set-env-vars $envVarsArray
    } else {
        Write-Host "Updating existing Container App for $($svc.Name)..." -ForegroundColor Green
        az containerapp update `
            --name $svc.Name `
            --resource-group $resourceGroup `
            --image "$acrServer/$($svc.Name):latest" `
            --set-env-vars $envVarsArray
    }
    
    # 3. Retrieve internal FQDN
    $fqdn = az containerapp show -n $svc.Name -g $resourceGroup --query properties.configuration.ingress.fqdn -o tsv
    Write-Host "✅ $($svc.Name) deployed at: https://$fqdn" -ForegroundColor Cyan
    $fqdns[$svc.ClusterId] = $fqdn
}

Write-Host "`n=================================================" -ForegroundColor Magenta
Write-Host "DEPLOYMENT COMPLETE. GATEWAY MAPPING REQUIRED" -ForegroundColor Yellow
Write-Host "=================================================" -ForegroundColor Magenta
Write-Host "To safely route traffic without breaking gatewayapi's existing settings, please go to the Azure Portal:"
Write-Host "1. Go to gatewayapi -> Containers -> Edit and deploy"
Write-Host "2. Add the following Environment Variables:" -ForegroundColor Cyan

foreach ($key in $fqdns.Keys) {
    Write-Host "`nName:  ReverseProxy__Clusters__$($key)__Destinations__destination1__Address"
    Write-Host "Value: https://$($fqdns[$key])/" -ForegroundColor Green
}

Write-Host "`nOnce you save these in gatewayapi, your entire application will be 100% online!" -ForegroundColor Yellow

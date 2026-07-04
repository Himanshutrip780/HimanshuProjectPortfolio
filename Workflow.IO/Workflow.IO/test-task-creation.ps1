$gatewayUrl = "https://gatewayapi.thankfulfield-a059e51b.centralindia.azurecontainerapps.io"

# 1. Register
$email = "test$((Get-Date).Ticks)@workflow.io.com"
$registerUrl = "$gatewayUrl/users/register"
$registerPayload = @{ email = $email; password = "Password123!"; firstName = "Test"; lastName = "User"; organizationName = "TestOrg" } | ConvertTo-Json
try {
    $regResponse = Invoke-RestMethod -Uri $registerUrl -Method Post -Body $registerPayload -ContentType "application/json"
    Write-Host "Registered user: $email"
} catch {
    Write-Host "Registration Failed: $_"
    exit
}

# 2. Login
$loginUrl = "$gatewayUrl/users/authenticate"
$loginPayload = @{ email = $email; password = "Password123!" } | ConvertTo-Json
try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginPayload -ContentType "application/json"
    $token = $loginResponse.data.jwtToken
    Write-Host "Logged in!"
} catch {
    Write-Host "Login Failed: $_"
    exit
}

$headers = @{ Authorization = "Bearer $token" }

# 3. Get Organization
$orgUrl = "$gatewayUrl/users/me/organization"
try {
    $orgResponse = Invoke-RestMethod -Uri $orgUrl -Method Get -Headers $headers
    $orgId = $orgResponse.data.organizationId
    Write-Host "Got Organization: $orgId"
} catch {
    Write-Host "Failed to get organization: $_"
    exit
}

# 4. Create Project
$headers.Add("X-Organization-ID", $orgId)
$headers.Add("X-Workspace-ID", "default")
$projUrl = "$gatewayUrl/projects"
$projPayload = @{
    name = "Test Project $((Get-Date).Ticks)"
    key = "K$((Get-Random -Minimum 1000 -Maximum 9999))"
    description = "A new project"
    visibility = 0
} | ConvertTo-Json

try {
    $projResponse = Invoke-RestMethod -Uri $projUrl -Method Post -Body $projPayload -ContentType "application/json" -Headers $headers
    $projectId = $projResponse.data.projectId
    Write-Host "Project created! $projectId"
} catch {
    Write-Host "Project creation failed: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
    exit
}

# 5. Create Task
$taskUrl = "$gatewayUrl/projects/$projectId/tasks"
$taskPayload = @{
    title = "Test task from PS"
    description = "Checking if 403 is gone"
    type = 0
    priority = 1
    projectId = $projectId
} | ConvertTo-Json

try {
    $taskResponse = Invoke-RestMethod -Uri $taskUrl -Method Post -Body $taskPayload -ContentType "application/json" -Headers $headers
    $taskId = $taskResponse.data.taskId
    Write-Host "Task created successfully! $taskId"

    # Query tasks list for verification
    Write-Host "Fetching project tasks..."
    $tasksList = Invoke-RestMethod -Uri $taskUrl -Method Get -Headers $headers
    $found = $tasksList.data | Where-Object { $_.taskId -eq $taskId }
    if ($found) {
        Write-Host "Verification Success: Task is present in the project tasks query! Title: $($found.title)" -ForegroundColor Green
    } else {
        Write-Host "Verification FAILED: Task is NOT returned by the project tasks query (hidden by query filters)!" -ForegroundColor Red
        Write-Host "Response Data:"
        $tasksList | ConvertTo-Json -Depth 5 | Write-Host
        exit
    }
} catch {
    Write-Host "Task creation failed: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

$gatewayUrl = "https://gatewayapi.thankfulfield-a059e51b.centralindia.azurecontainerapps.io"
$loginUrl = "$gatewayUrl/users/authenticate"

$loginPayload = @{
    email = "test@example.com"
    password = "password123"
} | ConvertTo-Json

# 1. Login to get token
Write-Host "Logging in..."
try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginPayload -ContentType "application/json"
    $token = $loginResponse.data.jwtToken
    Write-Host "Got token: $($token.Substring(0, 20))..."
} catch {
    Write-Host "Failed to login: $_"
    exit
}

# 2. Get Organization ID
Write-Host "Getting Organization..."
$orgUrl = "$gatewayUrl/users/me/organization"
$headers = @{
    Authorization = "Bearer $token"
}
try {
    $orgResponse = Invoke-RestMethod -Uri $orgUrl -Method Get -Headers $headers
    $orgId = $orgResponse.data.organizationId
    Write-Host "Got OrgId: $orgId"
    $headers.Add("X-Organization-ID", $orgId)
} catch {
    Write-Host "Failed to get org: $_"
    exit
}

# 3. Create Project
Write-Host "Creating Project..."
$createProjUrl = "$gatewayUrl/projects"
$projPayload = @{
    name = "Test Project"
    key = "TEST"
    projectType = 0
} | ConvertTo-Json

try {
    $projResponse = Invoke-RestMethod -Uri $createProjUrl -Method Post -Headers $headers -Body $projPayload -ContentType "application/json"
    $projectId = $projResponse.data.projectId
    Write-Host "Created Project: $projectId"
} catch {
    Write-Host "Failed to create project: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
    # Try getting existing project
    try {
        $projResponse = Invoke-RestMethod -Uri $createProjUrl -Method Get -Headers $headers
        $projectId = $projResponse.data[0].projectId
        Write-Host "Using existing project: $projectId"
    } catch {
        exit
    }
}

# 4. Create Task
Write-Host "Creating Task..."
$createTaskUrl = "$gatewayUrl/projects/$projectId/tasks"
$taskPayload = @{
    title = "My Test Task"
    description = "Test description"
    priority = 2
    issueType = 0
} | ConvertTo-Json

try {
    $taskResponse = Invoke-RestMethod -Uri $createTaskUrl -Method Post -Headers $headers -Body $taskPayload -ContentType "application/json"
    Write-Host "Created Task!"
    $taskResponse | ConvertTo-Json | Write-Host
} catch {
    Write-Host "Failed to create task: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

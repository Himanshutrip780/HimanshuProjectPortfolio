$gatewayUrl = "http://localhost:5270"

# 1. Register
Write-Host "Registering..."
$email = "test$(Get-Random)@example.com"
$regUrl = "$gatewayUrl/users/register"
$regPayload = @{
    email = $email
    password = "Password123!"
    firstName = "Test"
    lastName = "User"
} | ConvertTo-Json

try {
    $regResponse = Invoke-RestMethod -Uri $regUrl -Method Post -Body $regPayload -ContentType "application/json"
    Write-Host "Registered: $email"
} catch {
    Write-Host "Failed to register: $_"
    exit
}

# 2. Login
Write-Host "Logging in..."
$loginUrl = "$gatewayUrl/users/authenticate"
$loginPayload = @{
    email = $email
    password = "Password123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginPayload -ContentType "application/json"
    $token = $loginResponse.data.jwtToken
    Write-Host "Got token"
} catch {
    Write-Host "Failed to login: $_"
    exit
}

$headers = @{
    Authorization = "Bearer $token"
}

# 3. Get Organization
Write-Host "Getting Organization..."
$orgUrl = "$gatewayUrl/users/me/organization"
try {
    $orgResponse = Invoke-RestMethod -Uri $orgUrl -Method Get -Headers $headers
    $orgId = $orgResponse.data.organizationId
    Write-Host "Got OrgId: $orgId"
    $headers.Add("X-Organization-ID", $orgId)
} catch {
    Write-Host "Failed to get org: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
    exit
}

# 4. Create Project
Write-Host "Creating Project..."
$createProjUrl = "$gatewayUrl/projects"
$projPayload = @{
    name = "Test Project Local 4"
    key = "LOC4"
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
    exit
}

# 5. Create Task
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

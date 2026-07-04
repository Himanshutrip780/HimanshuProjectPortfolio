$tokenUrl = "http://localhost:5240/api/users/authenticate"
$tokenPayload = @{ email = "test@workflow.io.com"; password = "Password123!" } | ConvertTo-Json
try {
    $tokenResponse = Invoke-RestMethod -Uri $tokenUrl -Method Post -Body $tokenPayload -ContentType "application/json"
    $token = $tokenResponse.data.jwtToken
} catch {
    Write-Host "Login Failed: $_"
    exit
}

$headers = @{ Authorization = "Bearer $token" }

$projUrl = "http://localhost:5250/api/projects"
try {
    $projects = Invoke-RestMethod -Uri $projUrl -Method Get -Headers $headers
    $projectId = $projects.data[0].id
    Write-Host "Got Project ID: $projectId"
} catch {
    Write-Host "Failed to get project: $_"
    exit
}

$memberUrl1 = "http://localhost:5250/api/Project/$projectId/members"
try {
    Invoke-RestMethod -Uri $memberUrl1 -Method Get -Headers $headers
    Write-Host "api/Project/ works!"
} catch {
    Write-Host "api/Project/ failed: $_"
}

$memberUrl2 = "http://localhost:5250/api/projects/$projectId/members"
try {
    Invoke-RestMethod -Uri $memberUrl2 -Method Get -Headers $headers
    Write-Host "api/projects/ works!"
} catch {
    Write-Host "api/projects/ failed: $_"
}

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
} catch {
    Write-Host "Failed to get org: $_"
    exit
}

# 3. Hit /projects with X-Organization-ID
Write-Host "Hitting /projects..."
$projectsUrl = "$gatewayUrl/projects"
$headers.Add("X-Organization-ID", $orgId)
try {
    $projectsResponse = Invoke-RestMethod -Uri $projectsUrl -Method Get -Headers $headers
    Write-Host "Success! Projects:"
    $projectsResponse | ConvertTo-Json -Depth 5 | Write-Host
} catch {
    Write-Host "Failed to get projects: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

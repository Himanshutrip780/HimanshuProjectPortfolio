$gatewayUrl = "http://localhost:5270"

# 1. Login
$email = "test@example.com"
$loginUrl = "$gatewayUrl/users/authenticate"
$loginPayload = @{ email = $email; password = "Password123!" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginPayload -ContentType "application/json"
$token = $loginResponse.data.jwtToken

$headers = @{ Authorization = "Bearer $token" }

# 3. Try to GET Projects without X-Organization-ID
Write-Host "Getting Projects WITHOUT Org ID..."
$projectsUrl = "$gatewayUrl/projects"

try {
    $response = Invoke-RestMethod -Uri $projectsUrl -Method Get -Headers $headers
    Write-Host "Got Projects! Count: $($response.data.Count)"
} catch {
    Write-Host "Failed to get projects: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

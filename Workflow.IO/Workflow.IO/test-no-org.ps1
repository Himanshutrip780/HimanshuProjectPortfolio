$gatewayUrl = "http://localhost:5270"

# 1. Register
$email = "test$(Get-Random)@example.com"
$regUrl = "$gatewayUrl/users/register"
$regPayload = @{ email = $email; password = "Password123!"; firstName = "Test"; lastName = "User" } | ConvertTo-Json
Invoke-RestMethod -Uri $regUrl -Method Post -Body $regPayload -ContentType "application/json" | Out-Null

# 2. Login
$loginUrl = "$gatewayUrl/users/authenticate"
$loginPayload = @{ email = $email; password = "Password123!" } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginPayload -ContentType "application/json"
$token = $loginResponse.data.jwtToken

$headers = @{ Authorization = "Bearer $token" }

# 3. Try to Create Task without X-Organization-ID
Write-Host "Creating Task WITHOUT Org ID..."
$createTaskUrl = "$gatewayUrl/projects/b0d10f09-61e2-4cff-96d8-ecdbf4a1d158/tasks"
$taskPayload = @{ title = "Fail Task"; priority = 2 } | ConvertTo-Json

try {
    $taskResponse = Invoke-RestMethod -Uri $createTaskUrl -Method Post -Headers $headers -Body $taskPayload -ContentType "application/json"
    Write-Host "Created Task!"
} catch {
    Write-Host "Failed to create task: $_"
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "Response Body: $($reader.ReadToEnd())"
    }
}

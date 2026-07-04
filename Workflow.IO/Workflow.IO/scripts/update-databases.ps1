Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot

Push-Location $root
try {
    dotnet ef database update --project UserApi/UserApi.csproj --startup-project UserApi/UserApi.csproj --context UserDbContext
    dotnet ef database update --project UserApi/UserApi.csproj --startup-project UserApi/UserApi.csproj --context AuthDbContext
    dotnet ef database update --project ProjectApi/ProjectApi.csproj --startup-project ProjectApi/ProjectApi.csproj --context ProjectDbContext
    dotnet ef database update --project TaskApi/TaskApi.csproj --startup-project TaskApi/TaskApi.csproj --context TaskDbContext
    dotnet ef database update --project CommentApi/CommentApi.csproj --startup-project CommentApi/CommentApi.csproj --context CommentDbContext
    dotnet ef database update --project NotificationApi/NotificationApi.csproj --startup-project NotificationApi/NotificationApi.csproj --context NotificationDbContext
    dotnet ef database update --project ActivityApi/ActivityApi.csproj --startup-project ActivityApi/ActivityApi.csproj --context ActivityDbContext
    dotnet ef database update --project FileApi/FileApi.csproj --startup-project FileApi/FileApi.csproj --context FileDbContext
    dotnet ef database update --project AnalyticsApi/AnalyticsApi.csproj --startup-project AnalyticsApi/AnalyticsApi.csproj --context AnalyticsDbContext
}
finally {
    Pop-Location
}

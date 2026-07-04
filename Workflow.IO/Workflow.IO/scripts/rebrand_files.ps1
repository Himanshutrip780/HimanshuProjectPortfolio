$basePath = "c:\Users\Project\ZenTrack"

$excludePatterns = @("\\bin\\", "\\obj\\", "\\.git\\", "\\.vs\\", "\\node_modules\\", "\\.angular\\", "\\dist\\")

function ShouldProcess($path) {
    # Don't rename the root project folder itself to avoid breaking our current script context
    if ($path -eq $basePath) { return $false }
    foreach ($pattern in $excludePatterns) {
        if ($path -match $pattern) {
            return $false
        }
    }
    return $true
}

# 1. Rename files first
$filesToRename = Get-ChildItem -Path $basePath -Recurse -File | Where-Object { ShouldProcess($_.FullName) -and $_.Name -match "ZenTrack" -or $_.Name -match "zentrack" }
Write-Host "Found $($filesToRename.Count) files to rename."

foreach ($file in $filesToRename) {
    $newName = $file.Name
    $newName = $newName -creplace "ZenTrackHub", "WorkflowIOHub"
    $newName = $newName -creplace "ZenTrackApiExtensions", "WorkflowIOApiExtensions"
    $newName = $newName -creplace "ZenTrackContext", "WorkflowIOContext"
    $newName = $newName -creplace "ZenTrack", "Workflow.IO"
    $newName = $newName -creplace "zentrack", "workflow.io"
    
    if ($newName -ne $file.Name) {
        Rename-Item -Path $file.FullName -NewName $newName -PassThru
        Write-Host "Renamed file: $($file.Name) -> $newName"
    }
}

# 2. Rename directories (bottom-up to avoid path invalidation)
$dirsToRename = Get-ChildItem -Path $basePath -Recurse -Directory | Where-Object { ShouldProcess($_.FullName) -and $_.Name -match "ZenTrack" } | Sort-Object -Property @{Expression={$_.FullName.Length}; Descending=$true}
Write-Host "Found $($dirsToRename.Count) directories to rename."

foreach ($dir in $dirsToRename) {
    $newName = $dir.Name
    $newName = $newName -creplace "ZenTrack", "Workflow.IO"
    
    if ($newName -ne $dir.Name) {
        Rename-Item -Path $dir.FullName -NewName $newName -PassThru
        Write-Host "Renamed directory: $($dir.Name) -> $newName"
    }
}

Write-Host "File and directory renaming completed."

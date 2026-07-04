$basePath = "c:\Users\Project\Workflow.IO"

$excludePatterns = @("\\bin\\", "\\obj\\", "\\.git\\", "\\.vs\\", "\\node_modules\\", "\\.angular\\", "\\dist\\")

function ShouldProcess($path) {
    foreach ($pattern in $excludePatterns) {
        if ($path -match $pattern) {
            return $false
        }
    }
    return $true
}

$files = Get-ChildItem -Path $basePath -Recurse -File | Where-Object { ShouldProcess($_.FullName) }

Write-Host "Found $($files.Count) files. Starting text replacement..."

$count = 0
foreach ($file in $files) {
    # Skip image and binary files
    if ($file.Extension -match "\.(png|jpg|jpeg|gif|ico|dll|exe|pdb|cache|pack|idx)$") { continue }
    
    try {
        $content = Get-Content -Path $file.FullName -Raw -ErrorAction Stop
        if ($null -ne $content) {
            $newContent = $content -creplace "WorkflowIOHub", "WorkflowIOHub"
            $newContent = $newContent -creplace "WorkflowIOApiExtensions", "WorkflowIOApiExtensions"
            $newContent = $newContent -creplace "WorkflowIOContext", "WorkflowIOContext"
            $newContent = $newContent -creplace "Workflow.IO", "Workflow.IO"
            $newContent = $newContent -creplace "workflow.io", "workflow.io"
            $newContent = $newContent -creplace "WORKFLOW.IO", "WORKFLOW.IO"
            
            if ($content -cne $newContent) {
                Set-Content -Path $file.FullName -Value $newContent -NoNewline -Encoding UTF8
                $count++
                Write-Host "Updated $($file.FullName)"
            }
        }
    } catch {
        Write-Warning "Could not process file: $($file.FullName)"
    }
}

Write-Host "Text replacement completed. $count files updated."

# Dependency Download Script
# Automatically downloads required JAR files for Local Trading Server

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Local Trading Server - Dependency Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create libs directory
if (-not (Test-Path "libs")) {
    Write-Host "Creating libs directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "libs" | Out-Null
    Write-Host "✓ libs directory created" -ForegroundColor Green
} else {
    Write-Host "✓ libs directory exists" -ForegroundColor Green
}

Write-Host ""

# Define dependencies
$dependencies = @(
    @{
        Name = "AndroidAsync"
        Url = "https://repo1.maven.org/maven2/com/koushikdutta/async/androidasync/3.1.0/androidasync-3.1.0.jar"
        FileName = "androidasync-3.1.0.jar"
        Description = "Asynchronous HTTP server library by Koush"
    },
    @{
        Name = "JSON Library"
        Url = "https://repo1.maven.org/maven2/org/json/json/20210307/json-20210307.jar"
        FileName = "json-20210307.jar"
        Description = "JSON parsing and generation library"
    }
)

# Download each dependency
$allSuccess = $true

foreach ($dep in $dependencies) {
    Write-Host "Downloading: $($dep.Name)" -ForegroundColor Yellow
    Write-Host "  Description: $($dep.Description)" -ForegroundColor Gray
    Write-Host "  File: $($dep.FileName)" -ForegroundColor Gray
    
    $outputPath = "libs/$($dep.FileName)"
    
    # Check if already exists
    if (Test-Path $outputPath) {
        Write-Host "  ℹ File already exists" -ForegroundColor Cyan
        
        $overwrite = Read-Host "  Overwrite? (Y/N)"
        if ($overwrite -ne "Y" -and $overwrite -ne "y") {
            Write-Host "  ⊙ Skipped" -ForegroundColor Yellow
            Write-Host ""
            continue
        }
    }
    
    try {
        Write-Host "  Downloading from Maven Central..." -ForegroundColor Gray
        
        # Download with progress
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $dep.Url -OutFile $outputPath -ErrorAction Stop
        $ProgressPreference = 'Continue'
        
        # Verify file exists and has content
        if (Test-Path $outputPath) {
            $fileInfo = Get-Item $outputPath
            if ($fileInfo.Length -gt 0) {
                $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
                Write-Host "  ✓ Downloaded successfully ($sizeKB KB)" -ForegroundColor Green
            } else {
                Write-Host "  ✗ Downloaded file is empty" -ForegroundColor Red
                $allSuccess = $false
            }
        } else {
            Write-Host "  ✗ Download failed - file not created" -ForegroundColor Red
            $allSuccess = $false
        }
        
    } catch {
        Write-Host "  ✗ Download failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  Please download manually from: $($dep.Url)" -ForegroundColor Yellow
        $allSuccess = $false
    }
    
    Write-Host ""
}

# Summary
Write-Host "========================================" -ForegroundColor Cyan
if ($allSuccess) {
    Write-Host "All dependencies downloaded successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Run build script: .\build.ps1" -ForegroundColor Gray
    Write-Host "  2. Or compile manually: javac -cp `"libs/*`" -d bin *.java" -ForegroundColor Gray
    Write-Host "  3. Add StartLocalServer.cs to your Unity scene" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Ready to build!" -ForegroundColor Green
} else {
    Write-Host "Some downloads failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Please check the errors above and:" -ForegroundColor Yellow
    Write-Host "  1. Verify internet connection" -ForegroundColor Gray
    Write-Host "  2. Try manual download from Maven Central" -ForegroundColor Gray
    Write-Host "  3. See DEPENDENCIES.md for detailed instructions" -ForegroundColor Gray
    Write-Host ""
}

# List downloaded files
Write-Host "Current libs directory:" -ForegroundColor Cyan
if (Test-Path "libs") {
    $files = Get-ChildItem -Path "libs" -Filter "*.jar"
    if ($files.Count -gt 0) {
        foreach ($file in $files) {
            $sizeKB = [math]::Round($file.Length / 1KB, 2)
            Write-Host "  ✓ $($file.Name) ($sizeKB KB)" -ForegroundColor Green
        }
    } else {
        Write-Host "  No JAR files found" -ForegroundColor Yellow
    }
} else {
    Write-Host "  libs directory not found" -ForegroundColor Red
}

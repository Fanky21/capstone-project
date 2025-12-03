# Build script for Local Trading Server
# Compiles Java source files with required dependencies

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "Local Trading Server Build Script" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

# Check if Java is installed
Write-Host "Checking Java installation..." -ForegroundColor Yellow
try {
    $javaVersion = java -version 2>&1 | Select-String "version"
    Write-Host "✓ Java found: $javaVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Error: Java not found. Please install JDK 8 or higher." -ForegroundColor Red
    Write-Host "  Download from: https://www.oracle.com/java/technologies/downloads/" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Check if libs directory exists
if (-not (Test-Path "libs")) {
    Write-Host "Creating libs directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "libs" | Out-Null
}

# Check for required JAR files
Write-Host "Checking dependencies..." -ForegroundColor Yellow

$requiredJars = @(
    "androidasync*.jar",
    "json*.jar"
)

$missingJars = @()

foreach ($jar in $requiredJars) {
    $found = Get-ChildItem -Path "libs" -Filter $jar -ErrorAction SilentlyContinue
    if ($found) {
        Write-Host "✓ Found: $($found.Name)" -ForegroundColor Green
    } else {
        Write-Host "✗ Missing: $jar" -ForegroundColor Red
        $missingJars += $jar
    }
}

if ($missingJars.Count -gt 0) {
    Write-Host ""
    Write-Host "Missing dependencies detected!" -ForegroundColor Red
    Write-Host "Please download the following JAR files to the libs/ folder:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. AndroidAsync (by Koush)" -ForegroundColor Cyan
    Write-Host "   - GitHub: https://github.com/koush/AndroidAsync" -ForegroundColor Gray
    Write-Host "   - Maven: com.koushikdutta.async:androidasync:3.1.0" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. JSON Library" -ForegroundColor Cyan
    Write-Host "   - Maven: org.json:json:20210307" -ForegroundColor Gray
    Write-Host "   - Download: https://mvnrepository.com/artifact/org.json/json" -ForegroundColor Gray
    Write-Host ""
    
    $download = Read-Host "Would you like to see download instructions? (Y/N)"
    if ($download -eq "Y" -or $download -eq "y") {
        Write-Host ""
        Write-Host "Download Instructions:" -ForegroundColor Cyan
        Write-Host "=====================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Option 1: Maven Repository" -ForegroundColor Yellow
        Write-Host "  1. Visit https://mvnrepository.com/" -ForegroundColor Gray
        Write-Host "  2. Search for 'androidasync' and 'json'" -ForegroundColor Gray
        Write-Host "  3. Download the JAR files" -ForegroundColor Gray
        Write-Host "  4. Place them in the libs/ folder" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Option 2: Manual Download" -ForegroundColor Yellow
        Write-Host "  AndroidAsync:" -ForegroundColor Gray
        Write-Host "    https://github.com/koush/AndroidAsync/releases" -ForegroundColor Gray
        Write-Host "  JSON:" -ForegroundColor Gray
        Write-Host "    https://github.com/stleary/JSON-java/releases" -ForegroundColor Gray
    }
    
    exit 1
}

Write-Host ""

# Create bin directory if it doesn't exist
if (-not (Test-Path "bin")) {
    Write-Host "Creating bin directory..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path "bin" | Out-Null
}

# Build classpath
Write-Host "Building classpath..." -ForegroundColor Yellow
$jars = Get-ChildItem -Path "libs" -Filter "*.jar"
$classpath = ($jars | ForEach-Object { "libs/$($_.Name)" }) -join [IO.Path]::PathSeparator
    Write-Host "✓ Classpath built" -ForegroundColor Green

Write-Host ""

# Compile Java files
Write-Host "Compiling Java files..." -ForegroundColor Yellow

$javaFiles = @(
    "MoneyManager.java",
    "CompanyDataManager.java",
    "NewsManager.java",
    "LocalServer.java"
)

$compileSuccess = $true

foreach ($file in $javaFiles) {
    if (Test-Path $file) {
        Write-Host "  Compiling $file..." -ForegroundColor Gray
        
        $output = javac -cp "$classpath" -d "." -encoding UTF-8 $file 2>&1        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Error compiling $file" -ForegroundColor Red
            Write-Host $output -ForegroundColor Red
            $compileSuccess = $false
        } else {
            Write-Host "  ✓ $file compiled successfully" -ForegroundColor Green
        }
    } else {
        Write-Host "✗ File not found: $file" -ForegroundColor Red
        $compileSuccess = $false
    }
}

Write-Host ""

if ($compileSuccess) {
    Write-Host "==================================" -ForegroundColor Green
    Write-Host "Build Successful!" -ForegroundColor Green
    Write-Host "==================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Compiled classes are in the current directory" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To run the server manually:" -ForegroundColor Yellow
    Write-Host "  java -cp `".$([IO.Path]::PathSeparator)$classpath`" LocalServer" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Or use StartLocalServer.cs in Unity to start the server automatically." -ForegroundColor Cyan
} else {
    Write-Host "==================================" -ForegroundColor Red
    Write-Host "Build Failed!" -ForegroundColor Red
    Write-Host "==================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check the errors above and try again." -ForegroundColor Yellow
    exit 1
}

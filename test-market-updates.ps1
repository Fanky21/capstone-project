# Test Market Updates - Capstone Trading System
# Run this after installing APK to verify all fixes

Write-Host "`n=== CAPSTONE TRADING - MARKET UPDATE TEST ===" -ForegroundColor Cyan
Write-Host "Testing: Price updates, Buy/Sell, News generation`n" -ForegroundColor White

# Check device
Write-Host "[1/5] Checking device connection..." -ForegroundColor Yellow
$device = adb devices | Select-String "device$"
if (-not $device) {
    Write-Host "ERROR: No device connected!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Device connected`n" -ForegroundColor Green

# Clear old logs
Write-Host "[2/5] Clearing old logs..." -ForegroundColor Yellow
adb logcat -c
Write-Host "✓ Logs cleared`n" -ForegroundColor Green

# Launch app and capture initial state
Write-Host "[3/5] Launch app and monitor startup..." -ForegroundColor Yellow
Write-Host "Expected logs:" -ForegroundColor White
Write-Host "  - CompanyDataManager: Loaded 100 companies" -ForegroundColor Gray
Write-Host "  - Initial price update..." -ForegroundColor Gray
Write-Host "  - Initial news generation..." -ForegroundColor Gray

Start-Sleep -Seconds 2

# Monitor for 15 seconds
Write-Host "`n[4/5] Monitoring logs (15 seconds)...`n" -ForegroundColor Yellow

$monitorJob = Start-Job -ScriptBlock {
    adb logcat TradingServer:I CompanyDataManager:I NewsManager:I *:S
}

Start-Sleep -Seconds 15
Stop-Job $monitorJob
$logs = Receive-Job $monitorJob
Remove-Job $monitorJob

# Analyze logs
Write-Host "`n[5/5] Analyzing results...`n" -ForegroundColor Yellow

$companiesLoaded = $logs | Select-String "Loaded.*companies"
$initialUpdate = $logs | Select-String "Initial price update"
$initialNews = $logs | Select-String "Initial news"
$priceUpdate = $logs | Select-String "Prices updated"
$newsGen = $logs | Select-String "News generated"

if ($companiesLoaded) {
    Write-Host "✓ Companies loaded: $($companiesLoaded[0])" -ForegroundColor Green
} else {
    Write-Host "✗ Companies NOT loaded!" -ForegroundColor Red
}

if ($initialUpdate) {
    Write-Host "✓ Initial price update executed" -ForegroundColor Green
} else {
    Write-Host "✗ Initial price update MISSING!" -ForegroundColor Red
}

if ($initialNews) {
    Write-Host "✓ Initial news generated" -ForegroundColor Green
} else {
    Write-Host "✗ Initial news generation MISSING!" -ForegroundColor Red
}

if ($priceUpdate) {
    Write-Host "✓ Price updates working: $($priceUpdate.Count) updates" -ForegroundColor Green
} else {
    Write-Host "⚠ No price updates detected (normal if < 30s)" -ForegroundColor Yellow
}

if ($newsGen) {
    Write-Host "✓ News generation working: $($newsGen.Count) generations" -ForegroundColor Green
} else {
    Write-Host "⚠ No news generation detected (normal if < 30s)" -ForegroundColor Yellow
}

# Browser test instructions
Write-Host "`n=== MANUAL BROWSER TEST ===" -ForegroundColor Cyan
Write-Host "1. Open Chrome on device" -ForegroundColor White
Write-Host "2. Navigate to: http://localhost:5000" -ForegroundColor Yellow
Write-Host "3. Click 'Markets' tab" -ForegroundColor White
Write-Host "`n✓ Expected: See 100 companies with prices" -ForegroundColor Green
Write-Host "`n4. Click any company" -ForegroundColor White
Write-Host "5. Click 'BUY' button" -ForegroundColor White
Write-Host "`n✓ Expected: NO JavaScript errors in console" -ForegroundColor Green
Write-Host "`n6. Wait 10 seconds and refresh page" -ForegroundColor White
Write-Host "`n✓ Expected: Prices SHOULD BE DIFFERENT" -ForegroundColor Green
Write-Host "`n=== TEST COMPLETE ===`n" -ForegroundColor Cyan

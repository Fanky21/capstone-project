# Detailed test untuk debug price update
$baseUrl = "http://10.131.203.28:5000"

Write-Host "=== DETAILED PRICE UPDATE TEST ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Get initial data
Write-Host "[1] Fetching initial companies data..." -ForegroundColor Yellow
$response1 = Invoke-RestMethod -Uri "$baseUrl/api/companies"
$company1 = $response1[0]
$company2 = $response1[1]
$company3 = $response1[2]

Write-Host "  Company 1: $($company1.ticker)" -ForegroundColor White
Write-Host "    currentPrice: $($company1.currentPrice)" -ForegroundColor Gray
Write-Host "    previousPrice: $($company1.previousPrice)" -ForegroundColor Gray
Write-Host "    change: $($company1.change)" -ForegroundColor Gray
Write-Host "    changePercent: $($company1.changePercent)%" -ForegroundColor Gray

Write-Host "  Company 2: $($company2.ticker)" -ForegroundColor White
Write-Host "    currentPrice: $($company2.currentPrice)" -ForegroundColor Gray
Write-Host "    previousPrice: $($company2.previousPrice)" -ForegroundColor Gray
Write-Host "    change: $($company2.change)" -ForegroundColor Gray
Write-Host "    changePercent: $($company2.changePercent)%" -ForegroundColor Gray

Write-Host ""

# Test 2: Trigger manual update
Write-Host "[2] Triggering FIRST manual update..." -ForegroundColor Yellow
$update1 = Invoke-RestMethod -Uri "$baseUrl/api/update-prices" -Method POST
Write-Host "  Success: $($update1.success)" -ForegroundColor Green
Write-Host "  Companies count: $($update1.companies.Length)" -ForegroundColor Green

Write-Host ""

# Test 3: Get data after first update
Write-Host "[3] Fetching after FIRST update..." -ForegroundColor Yellow
$response2 = Invoke-RestMethod -Uri "$baseUrl/api/companies"
$company1_v2 = $response2[0]
$company2_v2 = $response2[1]

Write-Host "  Company 1: $($company1_v2.ticker)" -ForegroundColor White
Write-Host "    currentPrice: $($company1_v2.currentPrice) $(if ($company1.currentPrice -eq $company1_v2.currentPrice) { '❌ SAMA' } else { '✓ BERUBAH' })" -ForegroundColor $(if ($company1.currentPrice -eq $company1_v2.currentPrice) { 'Red' } else { 'Green' })
Write-Host "    previousPrice: $($company1_v2.previousPrice) $(if ($company1.currentPrice -eq $company1_v2.previousPrice) { '✓ Updated' } else { '❌ Not updated' })" -ForegroundColor Gray
Write-Host "    change: $($company1_v2.change)" -ForegroundColor Gray
Write-Host "    changePercent: $($company1_v2.changePercent)%" -ForegroundColor Gray

Write-Host "  Company 2: $($company2_v2.ticker)" -ForegroundColor White
Write-Host "    currentPrice: $($company2_v2.currentPrice) $(if ($company2.currentPrice -eq $company2_v2.currentPrice) { '❌ SAMA' } else { '✓ BERUBAH' })" -ForegroundColor $(if ($company2.currentPrice -eq $company2_v2.currentPrice) { 'Red' } else { 'Green' })
Write-Host "    previousPrice: $($company2_v2.previousPrice)" -ForegroundColor Gray
Write-Host "    change: $($company2_v2.change)" -ForegroundColor Gray
Write-Host "    changePercent: $($company2_v2.changePercent)%" -ForegroundColor Gray

Write-Host ""

# Test 4: Trigger second update
Write-Host "[4] Triggering SECOND manual update..." -ForegroundColor Yellow
$update2 = Invoke-RestMethod -Uri "$baseUrl/api/update-prices" -Method POST
Write-Host "  Success: $($update2.success)" -ForegroundColor Green

Write-Host ""

# Test 5: Get data after second update
Write-Host "[5] Fetching after SECOND update..." -ForegroundColor Yellow
$response3 = Invoke-RestMethod -Uri "$baseUrl/api/companies"
$company1_v3 = $response3[0]

Write-Host "  Company 1: $($company1_v3.ticker)" -ForegroundColor White
Write-Host "    currentPrice: $($company1_v3.currentPrice) $(if ($company1_v2.currentPrice -eq $company1_v3.currentPrice) { '❌ MASIH SAMA' } else { '✓ BERUBAH LAGI' })" -ForegroundColor $(if ($company1_v2.currentPrice -eq $company1_v3.currentPrice) { 'Red' } else { 'Green' })
Write-Host "    previousPrice: $($company1_v3.previousPrice) $(if ($company1_v2.currentPrice -eq $company1_v3.previousPrice) { '✓ Updated' } else { '❌ Not updated' })" -ForegroundColor Gray
Write-Host "    change: $($company1_v3.change)" -ForegroundColor Gray
Write-Host "    changePercent: $($company1_v3.changePercent)%" -ForegroundColor Gray

Write-Host ""
Write-Host "=== SUMMARY ===" -ForegroundColor Cyan

$allSame = ($company1.currentPrice -eq $company1_v2.currentPrice) -and ($company1_v2.currentPrice -eq $company1_v3.currentPrice)

if ($allSame) {
    Write-Host "❌ CRITICAL: Prices TIDAK BERUBAH sama sekali!" -ForegroundColor Red
    Write-Host "   Problem: updateAllPrices() tidak bekerja ATAU APK masih versi lama" -ForegroundColor Yellow
    Write-Host "   Solution:" -ForegroundColor Yellow
    Write-Host "   1. Pastikan Unity benar-benar rebuild (bukan resume)" -ForegroundColor White
    Write-Host "   2. Uninstall APK lama: adb uninstall com.DefaultCompany.capstoneproject" -ForegroundColor White
    Write-Host "   3. Install APK baru: adb install trading.apk" -ForegroundColor White
} else {
    Write-Host "✓ Prices BERUBAH! System working correctly" -ForegroundColor Green
    
    if ($company1_v2.change -eq 0 -or $company1_v3.change -eq 0) {
        Write-Host "⚠ Warning: Change calculation masih ada bug" -ForegroundColor Yellow
    } else {
        Write-Host "✓ Change calculation working!" -ForegroundColor Green
    }
}

Write-Host ""

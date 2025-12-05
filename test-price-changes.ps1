# Test script to monitor real-time price changes
Write-Host "=== MONITORING PRICE CHANGES ===" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"

# Get initial prices
Write-Host "[1] Fetching initial prices..." -ForegroundColor Yellow
try {
    $response1 = Invoke-RestMethod -Uri "$baseUrl/api/companies" -Method GET
    $first1 = $response1[0]
    Write-Host "  ✓ Initial: $($first1.ticker) = $($first1.currentPrice)" -ForegroundColor Green
    Write-Host "    Previous: $($first1.previousPrice)" -ForegroundColor Gray
    Write-Host "    Change: $($first1.changePercent)%" -ForegroundColor Gray
} catch {
    Write-Host "  ✗ Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[2] Waiting 10 seconds..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Get prices again
Write-Host "[3] Fetching prices again..." -ForegroundColor Yellow
try {
    $response2 = Invoke-RestMethod -Uri "$baseUrl/api/companies" -Method GET
    $first2 = $response2[0]
    Write-Host "  ✓ After 10s: $($first2.ticker) = $($first2.currentPrice)" -ForegroundColor Green
    Write-Host "    Previous: $($first2.previousPrice)" -ForegroundColor Gray
    Write-Host "    Change: $($first2.changePercent)%" -ForegroundColor Gray
} catch {
    Write-Host "  ✗ Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[4] Comparison:" -ForegroundColor Cyan
if ($first1.currentPrice -eq $first2.currentPrice) {
    Write-Host "  ⚠ STAGNAN - Price tidak berubah!" -ForegroundColor Red
    Write-Host "    Problem: Background thread mungkin tidak jalan" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ BERUBAH - Price berubah dari $($first1.currentPrice) ke $($first2.currentPrice)" -ForegroundColor Green
    $diff = [math]::Round($first2.currentPrice - $first1.currentPrice, 2)
    $pct = [math]::Round((($first2.currentPrice - $first1.currentPrice) / $first1.currentPrice) * 100, 2)
    Write-Host "    Difference: $diff ($pct%)" -ForegroundColor Green
}

Write-Host ""
Write-Host "[5] Waiting 35 seconds for background update..." -ForegroundColor Yellow
Start-Sleep -Seconds 35

# Get prices one more time (should have background update)
Write-Host "[6] Fetching after background update..." -ForegroundColor Yellow
try {
    $response3 = Invoke-RestMethod -Uri "$baseUrl/api/companies" -Method GET
    $first3 = $response3[0]
    Write-Host "  ✓ After 35s: $($first3.ticker) = $($first3.currentPrice)" -ForegroundColor Green
    Write-Host "    Previous: $($first3.previousPrice)" -ForegroundColor Gray
    Write-Host "    Change: $($first3.changePercent)%" -ForegroundColor Gray
} catch {
    Write-Host "  ✗ Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "[7] Final Comparison:" -ForegroundColor Cyan
if ($first2.currentPrice -eq $first3.currentPrice) {
    Write-Host "  ⚠ STAGNAN - Background thread tidak update!" -ForegroundColor Red
    Write-Host "    Solution: Cek logcat untuk error di background thread" -ForegroundColor Yellow
} else {
    Write-Host "  ✓ BERUBAH - Background thread berhasil update!" -ForegroundColor Green
    $diff = [math]::Round($first3.currentPrice - $first2.currentPrice, 2)
    $pct = [math]::Round((($first3.currentPrice - $first2.currentPrice) / $first2.currentPrice) * 100, 2)
    Write-Host "    Difference: $diff ($pct%)" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== TEST SELESAI ===" -ForegroundColor Cyan

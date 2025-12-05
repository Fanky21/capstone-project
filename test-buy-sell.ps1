# Quick Test - Buy/Sell Verification
Write-Host "`n=== BUY/SELL FUNCTIONALITY TEST ===" -ForegroundColor Cyan

Write-Host "`n[TEST 1] Server Status Check" -ForegroundColor Yellow
$response = try { Invoke-WebRequest -Uri "http://localhost:5000/api/companies" -TimeoutSec 5 } catch { $null }
if ($response) {
    $companies = ($response.Content | ConvertFrom-Json)
    Write-Host "✓ Server running - $($companies.Count) companies loaded" -ForegroundColor Green
} else {
    Write-Host "✗ Server not responding!" -ForegroundColor Red
    exit 1
}

Write-Host "`n[TEST 2] Portfolio Status" -ForegroundColor Yellow
$portfolio = (Invoke-WebRequest -Uri "http://localhost:5000/api/portfolio").Content | ConvertFrom-Json
Write-Host "Cash: Rp$($portfolio.cash)" -ForegroundColor White
Write-Host "Holdings: $($portfolio.portfolio.Count) stocks" -ForegroundColor White

Write-Host "`n[TEST 3] Execute Test Buy" -ForegroundColor Yellow
$testTicker = $companies[0].ticker
$testPrice = $companies[0].currentPrice
Write-Host "Buying 1 share of $testTicker @ Rp$testPrice..." -ForegroundColor White

$body = @{
    action = "buy"
    ticker = $testTicker
    shares = 1
} | ConvertTo-Json

try {
    $buyResult = Invoke-RestMethod -Uri "http://localhost:5000/api/trade" -Method POST -Body $body -ContentType "application/json"
    if ($buyResult.success) {
        Write-Host "✓ BUY SUCCESS: $($buyResult.message)" -ForegroundColor Green
        Write-Host "  New cash: Rp$($buyResult.cash)" -ForegroundColor Gray
    } else {
        Write-Host "✗ BUY FAILED: $($buyResult.error)" -ForegroundColor Red
    }
} catch {
    Write-Host "✗ BUY ERROR: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n[TEST 4] Verify Portfolio Updated" -ForegroundColor Yellow
Start-Sleep -Seconds 1
$newPortfolio = (Invoke-WebRequest -Uri "http://localhost:5000/api/portfolio").Content | ConvertFrom-Json
Write-Host "Cash: Rp$($newPortfolio.cash)" -ForegroundColor White
Write-Host "Holdings: $($newPortfolio.portfolio.Count) stocks" -ForegroundColor White

if ($newPortfolio.portfolio.Count -gt 0) {
    Write-Host "`n✓ Portfolio Items:" -ForegroundColor Green
    foreach ($item in $newPortfolio.portfolio) {
        Write-Host "  - $($item.ticker): $($item.shares) shares @ Rp$($item.avgPrice)" -ForegroundColor Gray
    }
} else {
    Write-Host "⚠ Portfolio still empty!" -ForegroundColor Yellow
}

Write-Host "`n[TEST 5] Check Price Updates" -ForegroundColor Yellow
$oldPrice = $companies[5].currentPrice
Write-Host "Stock $($companies[5].ticker) - Current: Rp$oldPrice" -ForegroundColor White
Write-Host "Triggering price update..." -ForegroundColor White

try {
    $updateResult = Invoke-RestMethod -Uri "http://localhost:5000/api/update-prices" -Method POST
    $newCompanies = $updateResult.companies
    $newPrice = ($newCompanies | Where-Object { $_.ticker -eq $companies[5].ticker }).currentPrice
    
    if ($newPrice -ne $oldPrice) {
        Write-Host "✓ PRICES UPDATED! New price: Rp$newPrice" -ForegroundColor Green
    } else {
        Write-Host "⚠ Price same (may need more time)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Update failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== TEST COMPLETE ===" -ForegroundColor Cyan
Write-Host "Check browser console for detailed logs!" -ForegroundColor White

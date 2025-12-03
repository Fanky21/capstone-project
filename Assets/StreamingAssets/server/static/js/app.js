// Flask API base URL
const API_BASE = '';

// Data
let companies = [];
let portfolio = { cash: 0, portfolio: [], totalValue: 0 };

// State management
let currentView = 'market';
let selectedStock = null;
let stockChart = null;
let tradingMode = 'buy';

// Custom Alert Function
function showAlert(message, type = 'warning') {
    const alertModal = document.getElementById('customAlert');
    const alertMessage = document.getElementById('alertMessage');
    const alertIcon = document.getElementById('alertIcon');
    const alertOkBtn = document.getElementById('alertOkBtn');
    
    alertMessage.textContent = message;
    
    // Set icon color based on type
    alertIcon.className = 'alert-icon ' + type;
    
    // Change icon based on type
    let iconSVG = '';
    if (type === 'success') {
        iconSVG = '<svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>';
    } else if (type === 'error') {
        iconSVG = '<svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>';
    } else if (type === 'info') {
        iconSVG = '<svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>';
    } else { // warning
        iconSVG = '<svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>';
    }
    alertIcon.innerHTML = iconSVG;
    
    alertModal.classList.remove('hidden');
    
    // Close on OK button
    alertOkBtn.onclick = () => {
        alertModal.classList.add('hidden');
    };
    
    // Close on click outside
    alertModal.onclick = (e) => {
        if (e.target === alertModal) {
            alertModal.classList.add('hidden');
        }
    };
}

// Initialize app
document.addEventListener('DOMContentLoaded', async () => {
    await loadCompanies();
    await loadPortfolio();
    setupEventListeners();
    
    // Render market view after data is loaded
    if (companies.length > 0) {
        renderMarketView();
    }
    
    startPriceUpdates();
});

// API calls
async function loadCompanies() {
    try {
        console.log('Loading companies...');
        const response = await fetch(`${API_BASE}/api/companies`);
        companies = await response.json();
        console.log(`Loaded ${companies.length} companies`);
    } catch (error) {
        console.error('Error loading companies:', error);
    }
}

async function loadPortfolio() {
    try {
        console.log('Loading portfolio...');
        const response = await fetch(`${API_BASE}/api/portfolio`);
        portfolio = await response.json();
        console.log('Portfolio loaded:', portfolio);
        updateCashDisplay();
    } catch (error) {
        console.error('Error loading portfolio:', error);
        // Initialize empty portfolio on error
        portfolio = { cash: 0, portfolio: [], totalValue: 0 };
    }
}

async function executeTrade(action, ticker, shares) {
    try {
        const response = await fetch(`${API_BASE}/api/trade`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ action, ticker, shares })
        });
        
        const result = await response.json();
        
        if (response.ok) {
            await loadPortfolio();
            return result;
        } else {
            throw new Error(result.error || 'Trade failed');
        }
    } catch (error) {
        console.error('Error executing trade:', error);
        throw error;
    }
}

async function updatePrices() {
    try {
        const response = await fetch(`${API_BASE}/api/update-prices`, {
            method: 'POST'
        });
        const result = await response.json();
        if (result.success) {
            companies = result.companies;
            console.log('Prices updated successfully - ' + companies.length + ' companies');
        }
    } catch (error) {
        console.error('Error updating prices:', error);
    }
}

// Event listeners setup
function setupEventListeners() {
    // Portfolio button
    const portfolioBtn = document.getElementById('portfolioBtn');
    if (portfolioBtn) {
        portfolioBtn.addEventListener('click', () => switchView('portfolio'));
    }
    
    // Markets button (when in portfolio view)
    const marketsBtn = document.getElementById('marketsBtn');
    if (marketsBtn) {
        marketsBtn.addEventListener('click', () => switchView('market'));
    }
    
    // Back button
    document.getElementById('backBtn').addEventListener('click', () => {
        document.getElementById('stockDetail').classList.add('hidden');
        document.getElementById('marketView').classList.remove('hidden');
    });
    
    // Trading buttons
    document.getElementById('buyBtn').addEventListener('click', () => openTradingModal('buy'));
    document.getElementById('sellBtn').addEventListener('click', () => openTradingModal('sell'));
    
    // Modal buttons
    document.getElementById('closeModal').addEventListener('click', closeTradingModal);
    document.getElementById('cancelBtn').addEventListener('click', closeTradingModal);
    document.getElementById('confirmBtn').addEventListener('click', handleTradeConfirm);
    
    // Shares input
    document.getElementById('sharesInput').addEventListener('input', updateTotalCost);
}

// Switch between views
function switchView(view) {
    currentView = view;
    
    // Update navigation active state
    document.querySelectorAll('.nav-btn').forEach(btn => btn.classList.remove('active'));
    
    if (view === 'market') {
        const marketsBtn = document.getElementById('marketsBtn');
        if (marketsBtn) marketsBtn.classList.add('active');
        renderMarketView();
    } else if (view === 'portfolio') {
        document.getElementById('portfolioBtn').classList.add('active');
        renderPortfolioView();
    }
}

// Render market view
function renderMarketView() {
    document.getElementById('marketView').classList.remove('hidden');
    document.getElementById('portfolioView').classList.add('hidden');
    document.getElementById('stockDetail').classList.add('hidden');
    
    const tbody = document.getElementById('stocksTableBody');
    tbody.innerHTML = '';
    
    companies.forEach(company => {
        const row = document.createElement('tr');
        const changePercent = ((company.currentPrice - company.previousPrice) / company.previousPrice * 100).toFixed(2);
        const isPositive = changePercent >= 0;
        
        row.innerHTML = `
            <td>
                <div class="company-cell">
                    <span class="company-ticker">${company.ticker}</span>
                    <span class="company-name">${company.name}</span>
                </div>
            </td>
            <td class="price">Rp${company.currentPrice.toLocaleString('id-ID', {minimumFractionDigits: 2})}</td>
            <td class="movement">${company.currentPrice > company.previousPrice ? '+' : ''}${(company.currentPrice - company.previousPrice).toFixed(2)}</td>
            <td>
                <span class="change ${isPositive ? 'positive' : 'negative'}">
                    <span class="arrow ${isPositive ? 'up' : 'down'}">${isPositive ? '▲' : '▼'}</span>
                    ${Math.abs(changePercent)}%
                </span>
            </td>
        `;
        
        row.addEventListener('click', () => showStockDetail(company.ticker));
        tbody.appendChild(row);
    });
    
    updateTicker();
}

// Show stock detail view
function showStockDetail(ticker) {
    const company = companies.find(c => c.ticker === ticker);
    if (!company) {
        console.error('Company not found:', ticker);
        return;
    }
    
    selectedStock = company;
    
    document.getElementById('marketView').classList.add('hidden');
    document.getElementById('stockDetail').classList.remove('hidden');
    
    // Update detail view
    document.getElementById('detailCompanyName').textContent = `${company.name} (${company.ticker})`;
    document.getElementById('companyDescription').textContent = company.description || 'No description available.';
    document.getElementById('detailTicker').textContent = company.ticker;
    document.getElementById('detailPrice').textContent = `Rp${company.currentPrice.toLocaleString('id-ID', {minimumFractionDigits: 2})}`;
    
    // Calculate stats
    const high = Math.max(...company.priceHistory);
    const low = Math.min(...company.priceHistory);
    const avg = company.priceHistory.reduce((a, b) => a + b, 0) / company.priceHistory.length;
    const changePercent = ((company.currentPrice - company.previousPrice) / company.previousPrice * 100).toFixed(2);
    
    document.getElementById('detailHigh').textContent = `Rp${high.toLocaleString('id-ID', {minimumFractionDigits: 2})}`;
    document.getElementById('detailLow').textContent = `Rp${low.toLocaleString('id-ID', {minimumFractionDigits: 2})}`;
    document.getElementById('detailAvg').textContent = `Rp${avg.toLocaleString('id-ID', {minimumFractionDigits: 2})}`;
    document.getElementById('detailChange').textContent = `${changePercent}%`;
    document.getElementById('detailChange').className = `info-value ${changePercent >= 0 ? 'green' : 'red'}`;
    
    // Additional info
    document.getElementById('detailPE').textContent = company.pe_ratio ? company.pe_ratio.toFixed(2) : '-';
    document.getElementById('detailDiv').textContent = company.dividend_yield ? `${company.dividend_yield.toFixed(2)}%` : '-';
    
    // Check portfolio ownership (safe check for undefined)
    const owned = portfolio && portfolio.portfolio ? portfolio.portfolio.find(p => p.ticker === company.ticker) : null;
    document.getElementById('detailOwned').textContent = owned ? owned.shares : 0;
    
    // Render chart
    renderStockChart(company);
}

// Render stock price chart
function renderStockChart(company) {
    const ctx = document.getElementById('stockChart').getContext('2d');
    
    if (stockChart) {
        stockChart.destroy();
    }
    
    const labels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
    
    stockChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Price',
                data: company.priceHistory,
                borderColor: '#3b82f6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                borderWidth: 3,
                tension: 0.4,
                fill: true,
                pointRadius: 4,
                pointBackgroundColor: '#3b82f6',
                pointBorderColor: '#ffffff',
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: '#1f2937',
                    titleColor: '#ffffff',
                    bodyColor: '#d1d5db',
                    borderColor: '#374151',
                    borderWidth: 2,
                    padding: 12,
                    displayColors: false,
                    callbacks: {
                        label: function(context) {
                            return 'Rp' + context.parsed.y.toLocaleString('id-ID', {minimumFractionDigits: 2});
                        }
                    }
                }
            },
            scales: {
                y: {
                    ticks: {
                        color: '#9ca3af',
                        callback: function(value) {
                            return 'Rp' + value.toFixed(0);
                        }
                    },
                    grid: {
                        color: '#374151'
                    }
                },
                x: {
                    ticks: {
                        color: '#9ca3af'
                    },
                    grid: {
                        color: '#374151'
                    }
                }
            }
        }
    });
}

// Render portfolio view
async function renderPortfolioView() {
    await loadPortfolio();
    
    document.getElementById('marketView').classList.add('hidden');
    document.getElementById('portfolioView').classList.remove('hidden');
    document.getElementById('stockDetail').classList.add('hidden');
    
    const tbody = document.getElementById('portfolioTableBody');
    tbody.innerHTML = '';
    
    if (portfolio.portfolio.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align: center; padding: 30px; color: #9ca3af;">No stocks in portfolio</td></tr>';
    } else {
        let totalProfit = 0;
        
        portfolio.portfolio.forEach(item => {
            const profit = item.profit;
            const profitPercent = item.profitPercent.toFixed(2);
            totalProfit += profit;
            
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>
                    <div class="company-cell">
                        <span class="company-ticker">${item.ticker}</span>
                        <span class="company-name">${item.name}</span>
                    </div>
                </td>
                <td class="price">${item.shares}</td>
                <td class="price">Rp${item.avgPrice.toLocaleString('id-ID', {minimumFractionDigits: 2})}</td>
                <td class="price">Rp${item.currentPrice.toLocaleString('id-ID', {minimumFractionDigits: 2})}</td>
                <td>
                    <span class="change ${profit >= 0 ? 'positive' : 'negative'}">
                        ${profit >= 0 ? '+' : ''}Rp${profit.toLocaleString('id-ID', {minimumFractionDigits: 2})} (${profitPercent}%)
                    </span>
                </td>
                <td>
                    <div class="portfolio-actions">
                        <button class="buy-btn-portfolio" data-ticker="${item.ticker}" data-price="${item.currentPrice}">Buy</button>
                        <button class="sell-btn-portfolio" data-ticker="${item.ticker}" data-shares="${item.shares}" data-price="${item.currentPrice}">Sell</button>
                    </div>
                </td>
            `;
            tbody.appendChild(row);
        });
        
        document.getElementById('portfolioProfit').textContent = `${totalProfit >= 0 ? '+' : ''}Rp${totalProfit.toLocaleString('id-ID', {minimumFractionDigits: 0})}`;
        document.getElementById('portfolioProfit').className = `stat-value ${totalProfit >= 0 ? 'green' : 'red'}`;
    }
    
    document.getElementById('portfolioValue').textContent = `Rp${portfolio.totalValue.toLocaleString('id-ID', {minimumFractionDigits: 0})}`;
    
    // Add event listeners to buy buttons
    document.querySelectorAll('.buy-btn-portfolio').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const ticker = e.target.dataset.ticker;
            const price = parseFloat(e.target.dataset.price);
            openPortfolioBuyModal(ticker, price);
        });
    });
    
    // Add event listeners to sell buttons
    document.querySelectorAll('.sell-btn-portfolio').forEach(btn => {
        btn.addEventListener('click', (e) => {
            const ticker = e.target.dataset.ticker;
            const shares = parseInt(e.target.dataset.shares);
            const price = parseFloat(e.target.dataset.price);
            openPortfolioSellModal(ticker, shares, price);
        });
    });
}

// Open trading modal
function openTradingModal(mode) {
    tradingMode = mode;
    
    if (mode === 'sell') {
        const owned = portfolio && portfolio.portfolio ? portfolio.portfolio.find(p => p.ticker === selectedStock.ticker) : null;
        if (!owned || owned.shares === 0) {
            showAlert('You don\'t own any shares of this stock!', 'warning');
            return;
        }
    }
    
    document.getElementById('modalTitle').textContent = mode === 'buy' ? 'Buy Stock' : 'Sell Stock';
    document.getElementById('modalStockName').textContent = `${selectedStock.ticker} - Rp${selectedStock.currentPrice.toLocaleString('id-ID', {minimumFractionDigits: 2})}`;
    document.getElementById('sharesInput').value = 1;
    updateTotalCost();
    
    document.getElementById('tradingModal').classList.remove('hidden');
}

// Close trading modal
function closeTradingModal() {
    document.getElementById('tradingModal').classList.add('hidden');
}

// Open buy modal from portfolio
function openPortfolioBuyModal(ticker, currentPrice) {
    const company = companies.find(c => c.ticker === ticker);
    if (!company) return;
    
    selectedStock = company;
    tradingMode = 'buy';
    
    document.getElementById('modalTitle').textContent = 'Buy Stock';
    document.getElementById('modalStockName').textContent = `${ticker} - Rp${currentPrice.toLocaleString('id-ID', {minimumFractionDigits: 2})}`;
    document.getElementById('sharesInput').value = 1;
    document.getElementById('sharesInput').removeAttribute('max');
    updateTotalCost();
    
    document.getElementById('tradingModal').classList.remove('hidden');
}

// Open sell modal from portfolio
function openPortfolioSellModal(ticker, maxShares, currentPrice) {
    const company = companies.find(c => c.ticker === ticker);
    if (!company) return;
    
    selectedStock = company;
    tradingMode = 'sell';
    
    document.getElementById('modalTitle').textContent = 'Sell Stock';
    document.getElementById('modalStockName').textContent = `${ticker} - Rp${currentPrice.toLocaleString('id-ID', {minimumFractionDigits: 2})}`;
    document.getElementById('sharesInput').value = maxShares;
    document.getElementById('sharesInput').max = maxShares;
    updateTotalCost();
    
    document.getElementById('tradingModal').classList.remove('hidden');
}

// Update total cost in modal
function updateTotalCost() {
    const shares = parseInt(document.getElementById('sharesInput').value) || 0;
    const total = shares * selectedStock.currentPrice;
    document.getElementById('totalCost').textContent = `Rp${total.toLocaleString('id-ID', {minimumFractionDigits: 2})}`;
}

// Handle trade confirmation
async function handleTradeConfirm() {
    const shares = parseInt(document.getElementById('sharesInput').value);
    
    if (!shares || shares <= 0) {
        showAlert('Please enter a valid number of shares!', 'warning');
        return;
    }
    
    try {
        console.log(`Executing trade: ${tradingMode} ${shares} shares of ${selectedStock.ticker}`);
        const result = await executeTrade(tradingMode, selectedStock.ticker, shares);
        
        console.log('Trade result:', result);
        
        // Determine alert type based on success
        const alertType = result.success ? 'success' : 'error';
        showAlert(result.message, alertType);
        
        // Refresh data after successful trade
        await loadPortfolio();
        await loadCompanies(); // Reload companies to get fresh prices
        
        console.log('Portfolio reloaded:', portfolio);
        
        // Re-render current view
        if (currentView === 'portfolio') {
            renderPortfolioView();
        } else {
            // Update market view and stock detail
            renderMarketView();
            if (selectedStock) {
                const updatedStock = companies.find(c => c.ticker === selectedStock.ticker);
                if (updatedStock) {
                    showStockDetail(updatedStock.ticker);
                }
            }
        }
        
        closeTradingModal();
    } catch (error) {
        console.error('Trade error:', error);
        showAlert(error.message || 'Trade failed. Please try again.', 'error');
    }
}

// Update cash display
function updateCashDisplay() {
    document.getElementById('cashAmount').textContent = `Rp${portfolio.cash.toLocaleString('id-ID', {minimumFractionDigits: 0})}`;
}

// Update ticker
function updateTicker() {
    const tickerContent = document.querySelector('.ticker-content');
    let html = '';
    
    companies.slice(0, 10).forEach(company => {
        const changePercent = ((company.currentPrice - company.previousPrice) / company.previousPrice * 100).toFixed(2);
        const isPositive = changePercent >= 0;
        const colorClass = isPositive ? 'green' : 'red';
        
        html += `<span class="ticker-item">${company.ticker} ${company.currentPrice.toFixed(2)} <span class="${colorClass}">${isPositive ? '▲' : '▼'}${Math.abs(changePercent)}%</span></span>`;
    });
    
    // Duplicate for seamless scroll
    tickerContent.innerHTML = html + html;
}

// Start automatic price updates
function startPriceUpdates() {
    setInterval(async () => {
        await updatePrices();
        
        // Re-render current view
        if (currentView === 'market' && !document.getElementById('stockDetail').classList.contains('hidden')) {
            const updatedStock = companies.find(c => c.ticker === selectedStock.ticker);
            if (updatedStock) {
                showStockDetail(updatedStock);
            }
        } else if (currentView === 'market') {
            renderMarketView();
        } else if (currentView === 'portfolio') {
            renderPortfolioView();
        }
        
        updateTicker();
    }, 10000); // Update every 10 seconds
}

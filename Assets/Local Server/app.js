// Data perusahaan - akan di-load dari companies.json
let companies = [];
let portfolio = JSON.parse(localStorage.getItem('portfolio')) || [];
let cash = parseFloat(localStorage.getItem('cash')) || 108654;

// State management
let currentView = 'market';
let selectedStock = null;
let stockChart = null;
let tradingMode = 'buy'; // 'buy' or 'sell'

// Initialize app
document.addEventListener('DOMContentLoaded', async () => {
    await loadCompanies();
    updateCashDisplay();
    setupEventListeners();
    renderMarketView();
    startPriceUpdates();
});

// Load companies data
async function loadCompanies() {
    try {
        const response = await fetch('companies.json');
        companies = await response.json();
        
        // Initialize price history for each company
        companies.forEach(company => {
            if (!company.priceHistory || company.priceHistory.length === 0) {
                company.priceHistory = generateInitialPriceHistory(company.currentPrice);
            }
        });
    } catch (error) {
        console.error('Error loading companies:', error);
        // Fallback to sample data if file doesn't exist
        companies = getSampleCompanies();
    }
}

// Generate initial price history for a stock
function generateInitialPriceHistory(currentPrice) {
    const history = [];
    let price = currentPrice * 1.2; // Start 20% higher
    
    for (let i = 0; i < 7; i++) {
        const change = (Math.random() - 0.5) * price * 0.15;
        price += change;
        price = Math.max(price, currentPrice * 0.5); // Don't go below 50% of current
        history.push(parseFloat(price.toFixed(2)));
    }
    
    return history;
}

// Event listeners setup
function setupEventListeners() {
    // Navigation
    document.getElementById('homeBtn').addEventListener('click', () => switchView('market'));
    document.getElementById('marketsBtn').addEventListener('click', () => switchView('market'));
    document.getElementById('portfolioBtn').addEventListener('click', () => switchView('portfolio'));
    
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
    document.getElementById('confirmBtn').addEventListener('click', executeTrade);
    
    // Shares input
    document.getElementById('sharesInput').addEventListener('input', updateTotalCost);
}

// Switch between views
function switchView(view) {
    currentView = view;
    
    // Update navigation active state
    document.querySelectorAll('.nav-btn').forEach(btn => btn.classList.remove('active'));
    
    if (view === 'market') {
        document.getElementById('homeBtn').classList.add('active');
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
            <td class="price">$${company.currentPrice.toFixed(2)}</td>
            <td class="movement">${company.currentPrice > company.previousPrice ? '+' : ''}${(company.currentPrice - company.previousPrice).toFixed(2)}</td>
            <td>
                <span class="change ${isPositive ? 'positive' : 'negative'}">
                    <span class="arrow ${isPositive ? 'up' : 'down'}">${isPositive ? '▲' : '▼'}</span>
                    ${Math.abs(changePercent)}%
                </span>
            </td>
        `;
        
        row.addEventListener('click', () => showStockDetail(company));
        tbody.appendChild(row);
    });
}

// Show stock detail view
function showStockDetail(company) {
    selectedStock = company;
    
    document.getElementById('marketView').classList.add('hidden');
    document.getElementById('stockDetail').classList.remove('hidden');
    
    // Update detail view
    document.getElementById('detailCompanyName').textContent = `${company.name} (${company.ticker})`;
    document.getElementById('companyDescription').textContent = company.description || 'No description available.';
    document.getElementById('detailPrice').textContent = `$${company.currentPrice.toFixed(2)}`;
    
    // Calculate stats
    const high = Math.max(...company.priceHistory);
    const low = Math.min(...company.priceHistory);
    const avg = company.priceHistory.reduce((a, b) => a + b, 0) / company.priceHistory.length;
    const changePercent = ((company.currentPrice - company.previousPrice) / company.previousPrice * 100).toFixed(2);
    
    document.getElementById('detailHigh').textContent = `$${high.toFixed(2)}`;
    document.getElementById('detailLow').textContent = `$${low.toFixed(2)}`;
    document.getElementById('detailAvg').textContent = `$${avg.toFixed(2)}`;
    document.getElementById('detailChange').textContent = `${changePercent}%`;
    document.getElementById('detailChange').className = `info-value ${changePercent >= 0 ? 'green' : 'red'}`;
    
    // Check portfolio ownership
    const owned = portfolio.find(p => p.ticker === company.ticker);
    document.getElementById('detailOwned').textContent = owned ? owned.shares : 0;
    
    // Render chart
    renderStockChart(company);
}

// Render stock price chart
function renderStockChart(company) {
    const ctx = document.getElementById('stockChart').getContext('2d');
    
    // Destroy previous chart if exists
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
                            return '$' + context.parsed.y.toFixed(2);
                        }
                    }
                }
            },
            scales: {
                y: {
                    ticks: {
                        color: '#9ca3af',
                        callback: function(value) {
                            return '$' + value.toFixed(0);
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
function renderPortfolioView() {
    document.getElementById('marketView').classList.add('hidden');
    document.getElementById('portfolioView').classList.remove('hidden');
    document.getElementById('stockDetail').classList.add('hidden');
    
    const tbody = document.getElementById('portfolioTableBody');
    tbody.innerHTML = '';
    
    let totalValue = cash;
    let totalProfit = 0;
    
    if (portfolio.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 30px; color: #9ca3af;">No stocks in portfolio</td></tr>';
    } else {
        portfolio.forEach(item => {
            const company = companies.find(c => c.ticker === item.ticker);
            if (!company) return;
            
            const currentValue = company.currentPrice * item.shares;
            const costBasis = item.avgPrice * item.shares;
            const profit = currentValue - costBasis;
            const profitPercent = (profit / costBasis * 100).toFixed(2);
            
            totalValue += currentValue;
            totalProfit += profit;
            
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>
                    <div class="company-cell">
                        <span class="company-ticker">${company.ticker}</span>
                        <span class="company-name">${company.name}</span>
                    </div>
                </td>
                <td class="price">${item.shares}</td>
                <td class="price">$${item.avgPrice.toFixed(2)}</td>
                <td class="price">$${company.currentPrice.toFixed(2)}</td>
                <td>
                    <span class="change ${profit >= 0 ? 'positive' : 'negative'}">
                        ${profit >= 0 ? '+' : ''}$${profit.toFixed(2)} (${profitPercent}%)
                    </span>
                </td>
            `;
            tbody.appendChild(row);
        });
    }
    
    document.getElementById('portfolioValue').textContent = `$${totalValue.toFixed(2)}`;
    document.getElementById('portfolioProfit').textContent = `${totalProfit >= 0 ? '+' : ''}$${totalProfit.toFixed(2)}`;
    document.getElementById('portfolioProfit').className = `stat-value ${totalProfit >= 0 ? 'green' : 'red'}`;
}

// Open trading modal
function openTradingModal(mode) {
    tradingMode = mode;
    
    if (mode === 'sell') {
        const owned = portfolio.find(p => p.ticker === selectedStock.ticker);
        if (!owned || owned.shares === 0) {
            alert('You don\'t own any shares of this stock!');
            return;
        }
    }
    
    document.getElementById('modalTitle').textContent = mode === 'buy' ? 'Buy Stock' : 'Sell Stock';
    document.getElementById('modalStockName').textContent = `${selectedStock.ticker} - $${selectedStock.currentPrice.toFixed(2)}`;
    document.getElementById('sharesInput').value = 1;
    updateTotalCost();
    
    document.getElementById('tradingModal').classList.remove('hidden');
}

// Close trading modal
function closeTradingModal() {
    document.getElementById('tradingModal').classList.add('hidden');
}

// Update total cost in modal
function updateTotalCost() {
    const shares = parseInt(document.getElementById('sharesInput').value) || 0;
    const total = shares * selectedStock.currentPrice;
    document.getElementById('totalCost').textContent = `$${total.toFixed(2)}`;
}

// Execute trade
function executeTrade() {
    const shares = parseInt(document.getElementById('sharesInput').value);
    
    if (!shares || shares <= 0) {
        alert('Please enter a valid number of shares!');
        return;
    }
    
    if (tradingMode === 'buy') {
        const cost = shares * selectedStock.currentPrice;
        
        if (cost > cash) {
            alert('Insufficient funds!');
            return;
        }
        
        // Deduct cash
        cash -= cost;
        
        // Add to portfolio
        const existing = portfolio.find(p => p.ticker === selectedStock.ticker);
        if (existing) {
            // Update average price
            const totalShares = existing.shares + shares;
            const totalCost = (existing.avgPrice * existing.shares) + cost;
            existing.avgPrice = totalCost / totalShares;
            existing.shares = totalShares;
        } else {
            portfolio.push({
                ticker: selectedStock.ticker,
                shares: shares,
                avgPrice: selectedStock.currentPrice
            });
        }
        
        alert(`Successfully bought ${shares} shares of ${selectedStock.ticker}!`);
    } else {
        // Sell
        const owned = portfolio.find(p => p.ticker === selectedStock.ticker);
        
        if (!owned || owned.shares < shares) {
            alert('You don\'t own enough shares!');
            return;
        }
        
        const revenue = shares * selectedStock.currentPrice;
        
        // Add cash
        cash += revenue;
        
        // Remove from portfolio
        owned.shares -= shares;
        if (owned.shares === 0) {
            portfolio = portfolio.filter(p => p.ticker !== selectedStock.ticker);
        }
        
        alert(`Successfully sold ${shares} shares of ${selectedStock.ticker}!`);
    }
    
    // Save to localStorage
    localStorage.setItem('cash', cash.toString());
    localStorage.setItem('portfolio', JSON.stringify(portfolio));
    
    // Update displays
    updateCashDisplay();
    showStockDetail(selectedStock);
    closeTradingModal();
}

// Update cash display
function updateCashDisplay() {
    document.getElementById('cashAmount').textContent = `$${cash.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 })}`;
}

// Start automatic price updates
function startPriceUpdates() {
    setInterval(() => {
        companies.forEach(company => {
            // Store previous price
            company.previousPrice = company.currentPrice;
            
            // Random price change (-5% to +5%)
            const changePercent = (Math.random() - 0.5) * 0.1;
            const newPrice = company.currentPrice * (1 + changePercent);
            company.currentPrice = Math.max(newPrice, 1); // Minimum $1
            
            // Update price history
            company.priceHistory.shift();
            company.priceHistory.push(company.currentPrice);
        });
        
        // Re-render current view
        if (currentView === 'market' && !document.getElementById('stockDetail').classList.contains('hidden')) {
            showStockDetail(selectedStock);
        } else if (currentView === 'market') {
            renderMarketView();
        } else if (currentView === 'portfolio') {
            renderPortfolioView();
        }
        
        updateTicker();
    }, 5000); // Update every 5 seconds
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

// Sample companies data (fallback)
function getSampleCompanies() {
    return [
        {
            ticker: 'AUG',
            name: 'Augury Insurance',
            currentPrice: 71.16,
            previousPrice: 71.50,
            description: 'Leading insurance provider in Liberty City.',
            priceHistory: [75, 73, 71, 69, 70, 72, 71.16],
            sector: 'Finance'
        },
        {
            ticker: 'BAN',
            name: 'Coolbeans',
            currentPrice: 57.82,
            previousPrice: 58.50,
            description: 'Popular coffee chain across San Andreas.',
            priceHistory: [60, 59, 58, 57, 58, 59, 57.82],
            sector: 'Consumer'
        },
        {
            ticker: 'BDG',
            name: 'BeanMachine',
            currentPrice: 45.92,
            previousPrice: 46.30,
            description: 'Artisan coffee and lifestyle brand.',
            priceHistory: [48, 47, 46, 45, 46, 47, 45.92],
            sector: 'Consumer'
        },
        {
            ticker: 'BGR',
            name: 'BurgerShot',
            currentPrice: 57.7,
            previousPrice: 63.09,
            description: 'Fast food restaurant chain specializing in burgers.',
            priceHistory: [65, 63, 61, 59, 58, 58, 57.7],
            sector: 'Consumer'
        },
        {
            ticker: 'BIL',
            name: 'Bilkinton',
            currentPrice: 88.09,
            previousPrice: 92.05,
            description: 'Makers of Pisswol, Bilkinton are a relative newcomer to the pharmaceutical market, fast catching up with the competition both in reported profits and open lawsuits.',
            priceHistory: [95, 92, 89, 87, 88, 90, 88.09],
            sector: 'Pharmaceutical'
        },
        {
            ticker: 'BNK',
            name: 'BankOfLiberty',
            currentPrice: 205.03,
            previousPrice: 206.65,
            description: 'One of the largest financial institutions.',
            priceHistory: [210, 208, 206, 204, 205, 207, 205.03],
            sector: 'Finance'
        },
        {
            ticker: 'BOM',
            name: 'BobMulet',
            currentPrice: 73.28,
            previousPrice: 79.22,
            description: 'Fashion and lifestyle brand.',
            priceHistory: [82, 80, 78, 76, 75, 74, 73.28],
            sector: 'Retail'
        },
        {
            ticker: 'BUL',
            name: 'Bullhead',
            currentPrice: 67.22,
            previousPrice: 81.68,
            description: 'Construction and heavy machinery.',
            priceHistory: [85, 83, 80, 78, 75, 70, 67.22],
            sector: 'Industrial'
        }
    ];
}

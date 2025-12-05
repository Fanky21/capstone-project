// Flask API base URL
const API_BASE = '';

// Data
let companies = [];
let news = [];
let oldNews = [];
let showingOldNews = false;
let portfolio = { cash: 0, portfolio: [], totalValue: 0 };

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
    await loadNews();
    await loadOldNews();
    await loadPortfolio();
    setupEventListeners();
    renderNews();
    startPriceUpdates();
    startNewsAutoRefresh();
});

// API calls
async function loadCompanies() {
    try {
        const response = await fetch(`${API_BASE}/api/companies`);
        companies = await response.json();
    } catch (error) {
        console.error('Error loading companies:', error);
    }
}

async function loadNews() {
    try {
        const response = await fetch(`${API_BASE}/api/news`);
        news = await response.json();
        console.log('News loaded:', news.length, 'articles');
        console.log('First news item:', news[0]);
        if (news[0]) {
            console.log('Has image?', !!news[0].image, news[0].image);
            console.log('Has summary?', !!news[0].summary, news[0].summary);
        }
    } catch (error) {
        console.error('Error loading news:', error);
    }
}

async function loadOldNews() {
    try {
        const response = await fetch(`${API_BASE}/api/news/old`);
        oldNews = await response.json();
    } catch (error) {
        console.error('Error loading old news:', error);
    }
}

async function loadPortfolio() {
    try {
        const response = await fetch(`${API_BASE}/api/portfolio`);
        portfolio = await response.json();
        updateCashDisplay();
    } catch (error) {
        console.error('Error loading portfolio:', error);
    }
}

async function updatePrices() {
    try {
        // Just fetch latest data (background thread already updates prices)
        const response = await fetch(`${API_BASE}/api/companies`);
        const result = await response.json();
        companies = result;
        console.log('Prices refreshed - ' + companies.length + ' companies');
    } catch (error) {
        console.error('Error updating prices:', error);
    }
}

// Event listeners setup
function setupEventListeners() {
    // Portfolio button
    document.getElementById('portfolioBtn').addEventListener('click', openPortfolioModal);
    document.getElementById('closePortfolio').addEventListener('click', closePortfolioModal);
    
    // News article detail modal
    const closeArticleBtn = document.getElementById('closeArticle');
    if (closeArticleBtn) {
        closeArticleBtn.addEventListener('click', closeArticleModal);
    }
}

// Render news
function renderNews() {
    // Render featured news (first article)
    if (news.length > 0) {
        const featured = news[0];
        const featuredContainer = document.getElementById('featuredNews');
        
        const featuredArticle = document.createElement('div');
        featuredArticle.className = 'featured-article';
        featuredArticle.style.cursor = 'pointer';
        featuredArticle.onclick = () => openArticleModal(featured);
        
        featuredArticle.innerHTML = `
            <div class="featured-image" style="background-image: url('${featured.image || 'https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?w=800'}')"></div>
            <div class="featured-content">
                <span class="news-category ${featured.category.toLowerCase().replace(/\s/g, '-')}">${featured.category}</span>
                <h2 class="news-title">${featured.title}</h2>
                <p class="news-summary">${featured.summary || featured.content || 'No summary available'}</p>
                <div class="news-meta">
                    <span class="news-date">${featured.date}</span>
                </div>
            </div>
        `;
        
        featuredContainer.innerHTML = '';
        featuredContainer.appendChild(featuredArticle);
    }
    
    // Render news grid (remaining articles)
    const newsGrid = document.getElementById('newsGrid');
    newsGrid.innerHTML = '';
    
    news.slice(1).forEach(article => {
        const card = document.createElement('div');
        card.className = 'news-card';
        card.style.cursor = 'pointer';
        card.onclick = () => openArticleModal(article);
        
        card.innerHTML = `
            <div class="news-image" style="background-image: url('${article.image || 'https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?w=800'}')"></div>
            <div class="news-card-content">
                <span class="news-category ${article.category.toLowerCase().replace(/\s/g, '-')}">${article.category}</span>
                <h3 class="news-card-title">${article.title}</h3>
                <p class="news-card-summary">${article.summary || article.content || 'No summary available'}</p>
                <span class="news-date">${article.date}</span>
            </div>
        `;
        
        newsGrid.appendChild(card);
    });
    
    // Add "Load More" button if there are old news and not showing them yet
    if (oldNews.length > 0 && !showingOldNews) {
        const loadMoreBtn = document.createElement('div');
        loadMoreBtn.className = 'load-more-container';
        loadMoreBtn.innerHTML = `
            <button class="load-more-btn" id="loadMoreBtn">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="6 9 12 15 18 9"></polyline>
                </svg>
                Lihat News Yang Lebih Lama (${oldNews.length})
            </button>
        `;
        newsGrid.appendChild(loadMoreBtn);
        
        document.getElementById('loadMoreBtn').addEventListener('click', showOldNews);
    }
    
    // Render old news if showing
    if (showingOldNews) {
        const oldNewsTitle = document.createElement('div');
        oldNewsTitle.className = 'old-news-title';
        oldNewsTitle.innerHTML = '<h3>News Lama</h3>';
        newsGrid.appendChild(oldNewsTitle);
        
        oldNews.forEach(article => {
            const card = document.createElement('div');
            card.className = 'news-card old-news-card';
            card.style.cursor = 'pointer';
            card.onclick = () => openArticleModal(article);
            
            card.innerHTML = `
                <div class="news-image" style="background-image: url('${article.image || 'https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?w=800'}')"></div>
                <div class="news-card-content">
                    <span class="news-category ${article.category.toLowerCase().replace(/\s/g, '-')}">${article.category}</span>
                    <h3 class="news-card-title">${article.title}</h3>
                    <p class="news-card-summary">${article.summary || article.content || 'No summary available'}</p>
                    <span class="news-date">${article.date}</span>
                </div>
            `;
            
            newsGrid.appendChild(card);
        });
        
        // Add "Hide" button
        const hideBtn = document.createElement('div');
        hideBtn.className = 'load-more-container';
        hideBtn.innerHTML = `
            <button class="load-more-btn" id="hideOldBtn">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="18 15 12 9 6 15"></polyline>
                </svg>
                Sembunyikan News Lama
            </button>
        `;
        newsGrid.appendChild(hideBtn);
        
        document.getElementById('hideOldBtn').addEventListener('click', hideOldNews);
    }
    
    updateTicker();
}

function showOldNews() {
    showingOldNews = true;
    renderNews();
    // Scroll to old news section
    document.querySelector('.old-news-title')?.scrollIntoView({ behavior: 'smooth' });
}

function hideOldNews() {
    showingOldNews = false;
    renderNews();
    // Scroll back to top of news grid
    document.getElementById('newsGrid').scrollIntoView({ behavior: 'smooth' });
}

// Portfolio modal
function openPortfolioModal() {
    renderPortfolioModal();
    document.getElementById('portfolioModal').classList.remove('hidden');
}

function closePortfolioModal() {
    document.getElementById('portfolioModal').classList.add('hidden');
}

async function renderPortfolioModal() {
    await loadPortfolio();
    
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
                    <a href="/markets" class="sell-btn-portfolio">View</a>
                </td>
            `;
            tbody.appendChild(row);
        });
        
        document.getElementById('portfolioProfit').textContent = `${totalProfit >= 0 ? '+' : ''}Rp${totalProfit.toLocaleString('id-ID', {minimumFractionDigits: 0})}`;
        document.getElementById('portfolioProfit').className = `stat-value ${totalProfit >= 0 ? 'green' : 'red'}`;
    }
    
    document.getElementById('portfolioValue').textContent = `Rp${portfolio.totalValue.toLocaleString('id-ID', {minimumFractionDigits: 0})}`;
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

// Article modal functions
function openArticleModal(article) {
    const modal = document.getElementById('articleModal');
    const articleContent = document.getElementById('articleContent');
    
    // Generate full article content (since we only have summary, we'll use it as the full content for now)
    const fullContent = `
        <div class="article-header">
            <span class="news-category ${article.category.toLowerCase().replace(/\s/g, '-')}">${article.category}</span>
            <h2>${article.title}</h2>
            <div class="article-meta">
                <span class="news-date">${article.date}</span>
            </div>
        </div>
        <div class="article-image" style="background-image: url('${article.image}')"></div>
        <div class="article-body">
            <p>${article.summary}</p>
            <p>Pasar saham Indonesia terus menunjukkan dinamika yang menarik dengan berbagai faktor yang mempengaruhi pergerakan harga. Para investor disarankan untuk terus memantau perkembangan dan melakukan analisis menyeluruh sebelum mengambil keputusan investasi.</p>
            <p>Dengan volatilitas yang ada, baik investor jangka panjang maupun trader aktif perlu memperhatikan fundamental perusahaan dan tren pasar global yang dapat berdampak pada bursa domestik.</p>
        </div>
    `;
    
    articleContent.innerHTML = fullContent;
    modal.classList.remove('hidden');
}

function closeArticleModal() {
    const modal = document.getElementById('articleModal');
    modal.classList.add('hidden');
}

// Start automatic price updates
function startPriceUpdates() {
    setInterval(async () => {
        await updatePrices();
        updateTicker();
    }, 10000); // Update every 10 seconds
}

// Start automatic news refresh
function startNewsAutoRefresh() {
    setInterval(async () => {
        const previousNewsCount = news.length;
        await loadNews();
        await loadOldNews();
        
        // Only re-render if news has changed
        if (news.length !== previousNewsCount || news[0]?.id !== previousNewsCount) {
            renderNews();
            
            // Show notification for new news
            if (news.length > 0 && news[0].id) {
                showNewsNotification();
            }
        }
    }, 5000); // Check for new news every 5 seconds
}

// Show notification when new news arrives
function showNewsNotification() {
    const notification = document.createElement('div');
    notification.className = 'news-notification';
    notification.innerHTML = `
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
            <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
        </svg>
        <span>News baru tersedia!</span>
    `;
    
    document.body.appendChild(notification);
    
    // Remove notification after 3 seconds
    setTimeout(() => {
        notification.classList.add('fade-out');
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

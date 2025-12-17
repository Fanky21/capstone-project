// Use relative URL so it works from any origin
const API_BASE = window.location.origin;

// State
let balance = 0;
let deposits = [];
let loans = [];

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    console.log('🏦 Bank Compunders initialized');
    
    setupEventListeners();
    loadInitialData();
    startAutoRefresh();
});

// Setup Event Listeners
function setupEventListeners() {
    // Tab switching
    document.querySelectorAll('.tab-button').forEach(btn => {
        btn.addEventListener('click', () => switchTab(btn.dataset.tab));
    });
    
    // Deposit form
    const depositForm = document.getElementById('depositForm');
    const depositAmount = document.getElementById('depositAmount');
    const depositDays = document.getElementById('depositDays');
    
    depositAmount.addEventListener('input', updateDepositPreview);
    depositDays.addEventListener('input', () => {
        document.getElementById('daysDisplay').textContent = depositDays.value;
        updateDepositPreview();
    });
    
    depositForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        await createDeposit();
    });
    
    // Loan button
    document.getElementById('createLoanBtn').addEventListener('click', async () => {
        await createLoan();
    });
}

// Tab Switching
function switchTab(tabName) {
    // Update buttons
    document.querySelectorAll('.tab-button').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.tab === tabName);
    });
    
    // Update content
    document.querySelectorAll('.tab-content').forEach(content => {
        const isDeposit = content.id === 'depositTab';
        const shouldShow = (tabName === 'deposit' && isDeposit) || (tabName === 'loan' && !isDeposit);
        content.classList.toggle('active', shouldShow);
    });
}

// Load Initial Data
async function loadInitialData() {
    await Promise.all([
        loadBalance(),
        loadDeposits(),
        loadLoans(),
        loadStats()
    ]);
}

// Load Balance
async function loadBalance() {
    try {
        const response = await fetch(`${API_BASE}/api/balance`);
        const data = await response.json();
        balance = data.balance;
        updateBalanceDisplay();
    } catch (error) {
        console.error('Failed to load balance:', error);
    }
}

// Load Deposits
async function loadDeposits() {
    try {
        const response = await fetch(`${API_BASE}/api/deposits`);
        deposits = await response.json();
        renderDeposits();
    } catch (error) {
        console.error('Failed to load deposits:', error);
    }
}

// Load Loans
async function loadLoans() {
    try {
        const response = await fetch(`${API_BASE}/api/loans`);
        loans = await response.json();
        renderLoans();
    } catch (error) {
        console.error('Failed to load loans:', error);
    }
}

// Load Stats
async function loadStats() {
    try {
        const response = await fetch(`${API_BASE}/api/stats`);
        const stats = await response.json();
        renderStats(stats);
    } catch (error) {
        console.error('Failed to load stats:', error);
    }
}

// Update Balance Display
function updateBalanceDisplay() {
    document.getElementById('balanceAmount').textContent = formatCurrency(balance);
}

// Update Deposit Preview
function updateDepositPreview() {
    const amount = parseFloat(document.getElementById('depositAmount').value) || 0;
    const days = parseInt(document.getElementById('depositDays').value);
    
    // Calculate interest (6% per year)
    const dailyRate = 0.06 / 365;
    const interest = amount * dailyRate * days;
    const total = amount + interest;
    
    document.getElementById('previewPrincipal').textContent = formatCurrency(amount);
    document.getElementById('previewInterest').textContent = formatCurrency(interest);
    document.getElementById('previewTotal').textContent = formatCurrency(total);
}

// Create Deposit
async function createDeposit() {
    const amount = parseFloat(document.getElementById('depositAmount').value);
    const days = parseInt(document.getElementById('depositDays').value);
    
    if (!amount || amount <= 0) {
        alert('Masukkan jumlah deposito yang valid');
        return;
    }
    
    if (amount > balance) {
        alert('Saldo tidak mencukupi');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE}/api/deposits/create`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ amount, days })
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('✅ Deposito berhasil dibuat!');
            
            // Reset form
            document.getElementById('depositForm').reset();
            document.getElementById('daysDisplay').textContent = '7';
            updateDepositPreview();
            
            // Reload data
            await loadInitialData();
        } else {
            alert('❌ ' + result.error);
        }
    } catch (error) {
        console.error('Failed to create deposit:', error);
        alert('Terjadi kesalahan. Silakan coba lagi.');
    }
}

// Withdraw Deposit
async function withdrawDeposit(depositId) {
    if (!confirm('Tarik deposito ini?')) return;
    
    try {
        const response = await fetch(`${API_BASE}/api/deposits/withdraw/${depositId}`, {
            method: 'POST'
        });
        
        const result = await response.json();
        
        if (result.success) {
            const msg = result.isMature 
                ? `✅ Deposito jatuh tempo! Diterima: ${formatCurrency(result.returnAmount)}`
                : `⚠️ Penarikan awal (tanpa bunga). Diterima: ${formatCurrency(result.returnAmount)}`;
            alert(msg);
            
            // Reload data
            await loadInitialData();
        } else {
            alert('❌ ' + result.error);
        }
    } catch (error) {
        console.error('Failed to withdraw deposit:', error);
        alert('Terjadi kesalahan. Silakan coba lagi.');
    }
}

// Create Loan
async function createLoan() {
    if (loans.length > 0) {
        alert('Anda sudah memiliki pinjaman aktif');
        return;
    }
    
    if (!confirm('Ajukan pinjaman Rp 100.000 dengan pengembalian Rp 120.000 dalam 30 hari?')) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE}/api/loans/create`, {
            method: 'POST'
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert('✅ Pinjaman disetujui! Rp 100.000 telah ditambahkan ke saldo Anda.');
            
            // Reload data
            await loadInitialData();
        } else {
            alert('❌ ' + result.error);
        }
    } catch (error) {
        console.error('Failed to create loan:', error);
        alert('Terjadi kesalahan. Silakan coba lagi.');
    }
}

// Repay Loan
async function repayLoan(loanId) {
    if (!confirm('Bayar pinjaman sekarang?')) return;
    
    try {
        const response = await fetch(`${API_BASE}/api/loans/repay/${loanId}`, {
            method: 'POST'
        });
        
        const result = await response.json();
        
        if (result.success) {
            alert(`✅ Pinjaman berhasil dibayar! Terpotong: ${formatCurrency(result.repaidAmount)}`);
            
            // Reload data
            await loadInitialData();
        } else {
            alert('❌ ' + result.error);
        }
    } catch (error) {
        console.error('Failed to repay loan:', error);
        alert('Terjadi kesalahan. Silakan coba lagi.');
    }
}

// Render Deposits
function renderDeposits() {
    const container = document.getElementById('depositsList');
    
    if (deposits.length === 0) {
        container.innerHTML = '<div class="items-list empty">Belum ada deposito aktif</div>';
        container.className = 'items-list empty';
        return;
    }
    
    container.className = 'items-list';
    container.innerHTML = deposits.map(deposit => {
        const progress = Math.max(0, ((deposit.days - deposit.daysRemaining) / deposit.days) * 100);
        const statusClass = deposit.isMature ? 'mature' : 'active';
        const statusText = deposit.isMature ? 'Jatuh Tempo' : 'Aktif';
        
        return `
            <div class="item">
                <div class="item-header">
                    <span class="item-id">${deposit.id}</span>
                    <span class="item-status ${statusClass}">${statusText}</span>
                </div>
                <div class="item-details">
                    <div class="item-detail">
                        <span>Pokok</span>
                        <span>${formatCurrency(deposit.amount)}</span>
                    </div>
                    <div class="item-detail">
                        <span>Bunga</span>
                        <span class="text-success">+${formatCurrency(deposit.interest)}</span>
                    </div>
                    <div class="item-detail">
                        <span>Total Kembali</span>
                        <span>${formatCurrency(deposit.totalReturn)}</span>
                    </div>
                    <div class="item-detail">
                        <span>Tenor</span>
                        <span>${deposit.days} hari</span>
                    </div>
                </div>
                <div class="item-progress">
                    <div class="progress-bar">
                        <div class="progress-fill ${deposit.isMature ? 'complete' : ''}" style="width: ${progress}%"></div>
                    </div>
                    <div class="progress-label">
                        <span>${deposit.daysRemaining} hari tersisa</span>
                        <span>${Math.round(progress)}%</span>
                    </div>
                </div>
                <button class="btn ${deposit.isMature ? 'btn-success' : 'btn-primary'}" 
                        onclick="withdrawDeposit('${deposit.id}')">
                    ${deposit.isMature ? '✓ Tarik Deposito' : 'Tarik Awal (Tanpa Bunga)'}
                </button>
            </div>
        `;
    }).join('');
}

// Render Loans
function renderLoans() {
    const container = document.getElementById('loansList');
    
    if (loans.length === 0) {
        container.innerHTML = '<div class="items-list empty">Belum ada pinjaman aktif</div>';
        container.className = 'items-list empty';
        return;
    }
    
    container.className = 'items-list';
    container.innerHTML = loans.map(loan => {
        const progress = Math.max(0, ((30 - loan.daysRemaining) / 30) * 100);
        const statusClass = loan.isOverdue ? 'overdue' : 'active';
        const statusText = loan.isOverdue ? 'Jatuh Tempo' : 'Aktif';
        
        return `
            <div class="item">
                <div class="item-header">
                    <span class="item-id">${loan.id}</span>
                    <span class="item-status ${statusClass}">${statusText}</span>
                </div>
                <div class="item-details">
                    <div class="item-detail">
                        <span>Jumlah Pinjaman</span>
                        <span>${formatCurrency(loan.amount)}</span>
                    </div>
                    <div class="item-detail">
                        <span>Bunga (20%)</span>
                        <span class="text-danger">+${formatCurrency(loan.amount * 0.2)}</span>
                    </div>
                    <div class="item-detail">
                        <span>Total Bayar</span>
                        <span>${formatCurrency(loan.repaymentAmount)}</span>
                    </div>
                    <div class="item-detail">
                        <span>Jatuh Tempo</span>
                        <span>${new Date(loan.dueDate).toLocaleDateString('id-ID')}</span>
                    </div>
                </div>
                <div class="item-progress">
                    <div class="progress-bar">
                        <div class="progress-fill" style="width: ${progress}%"></div>
                    </div>
                    <div class="progress-label">
                        <span>${loan.daysRemaining} hari tersisa</span>
                        <span>${Math.round(progress)}%</span>
                    </div>
                </div>
                ${loan.isOverdue ? 
                    '<div class="loan-warning">⚠️ Pinjaman jatuh tempo! Akan otomatis dipotong dari saldo</div>' :
                    `<button class="btn btn-primary" onclick="repayLoan('${loan.id}')">Bayar Sekarang</button>`
                }
            </div>
        `;
    }).join('');
}

// Render Stats
function renderStats(stats) {
    const statsHtml = `
        <div class="stat">
            <span class="stat-value">${stats.activeDeposits}</span>
            <span class="stat-label">Deposito</span>
        </div>
        <div class="stat">
            <span class="stat-value">${stats.activeLoans}</span>
            <span class="stat-label">Pinjaman</span>
        </div>
        <div class="stat">
            <span class="stat-value">${formatCurrencyShort(stats.totalDeposited)}</span>
            <span class="stat-label">Total Deposito</span>
        </div>
    `;
    document.getElementById('statsDisplay').innerHTML = statsHtml;
}

// Auto Refresh
function startAutoRefresh() {
    setInterval(async () => {
        await loadInitialData();
        console.log('🔄 Data refreshed');
    }, 10000); // Every 10 seconds
}

// Utility Functions
function formatCurrency(amount) {
    return 'Rp ' + Math.round(amount).toLocaleString('id-ID');
}

function formatCurrencyShort(amount) {
    if (amount >= 1000000) {
        return 'Rp ' + (amount / 1000000).toFixed(1) + 'M';
    } else if (amount >= 1000) {
        return 'Rp ' + (amount / 1000).toFixed(0) + 'K';
    }
    return formatCurrency(amount);
}

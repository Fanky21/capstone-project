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
    setupCustomAlert();
});

// Custom Alert Functions
function setupCustomAlert() {
    const alertBtn = document.getElementById('alertBtn');
    const alertOverlay = document.getElementById('customAlert');
    
    alertBtn.addEventListener('click', hideCustomAlert);
    alertOverlay.addEventListener('click', (e) => {
        if (e.target === alertOverlay) hideCustomAlert();
    });
    
    // Setup confirm modal
    const confirmOkBtn = document.getElementById('confirmOkBtn');
    const confirmCancelBtn = document.getElementById('confirmCancelBtn');
    const confirmOverlay = document.getElementById('customConfirm');
    
    confirmCancelBtn.addEventListener('click', () => hideCustomConfirm(false));
    confirmOverlay.addEventListener('click', (e) => {
        if (e.target === confirmOverlay) hideCustomConfirm(false);
    });
}

function showCustomAlert(message, title = 'Notifikasi', type = 'success') {
    const modal = document.getElementById('customAlert');
    const icon = document.getElementById('alertIcon');
    const titleEl = document.getElementById('alertTitle');
    const messageEl = document.getElementById('alertMessage');
    const btn = document.getElementById('alertBtn');
    
    // Set icon and color based on type
    const configs = {
        success: { icon: '✓', color: '#10b981', title: title || 'Berhasil' },
        error: { icon: '✗', color: '#ef4444', title: title || 'Gagal' },
        warning: { icon: '⚠', color: '#f59e0b', title: title || 'Peringatan' },
        info: { icon: 'ℹ', color: '#3b82f6', title: title || 'Informasi' }
    };
    
    const config = configs[type] || configs.info;
    
    icon.textContent = config.icon;
    icon.style.backgroundColor = config.color;
    titleEl.textContent = config.title;
    messageEl.textContent = message;
    btn.style.backgroundColor = config.color;
    
    modal.classList.add('show');
}

function hideCustomAlert() {
    const modal = document.getElementById('customAlert');
    modal.classList.remove('show');
}

let confirmResolve = null;

function showCustomConfirm(message, title = 'Konfirmasi', type = 'info') {
    return new Promise((resolve) => {
        const modal = document.getElementById('customConfirm');
        const icon = document.getElementById('confirmIcon');
        const titleEl = document.getElementById('confirmTitle');
        const messageEl = document.getElementById('confirmMessage');
        const okBtn = document.getElementById('confirmOkBtn');
        const cancelBtn = document.getElementById('confirmCancelBtn');
        
        // Set icon and color based on type
        const configs = {
            warning: { icon: '⚠', color: '#f59e0b' },
            danger: { icon: '!', color: '#ef4444' },
            info: { icon: '?', color: '#3b82f6' },
            success: { icon: 'i', color: '#10b981' }
        };
        
        const config = configs[type] || configs.info;
        
        icon.textContent = config.icon;
        icon.style.backgroundColor = config.color;
        titleEl.textContent = title;
        messageEl.textContent = message;
        okBtn.style.backgroundColor = config.color;
        
        // Store resolve function
        confirmResolve = resolve;
        
        // Setup OK button
        const handleOk = () => {
            okBtn.removeEventListener('click', handleOk);
            hideCustomConfirm(true);
        };
        okBtn.addEventListener('click', handleOk);
        
        modal.classList.add('show');
    });
}

function hideCustomConfirm(result) {
    const modal = document.getElementById('customConfirm');
    modal.classList.remove('show');
    if (confirmResolve) {
        confirmResolve(result);
        confirmResolve = null;
    }
}

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
        showCustomAlert('Masukkan jumlah deposito yang valid', 'Input Tidak Valid', 'warning');
        return;
    }
    
    if (amount > balance) {
        showCustomAlert('Saldo tidak mencukupi untuk membuat deposito', 'Saldo Kurang', 'error');
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
            const message = `Deposito ${formatCurrency(amount)} untuk ${days} hari berhasil dibuat!\nTotal kembali: ${formatCurrency(result.deposit.totalReturn)}`;
            showCustomAlert(message, 'Deposito Berhasil', 'success');
            
            // Reset form
            document.getElementById('depositForm').reset();
            document.getElementById('daysDisplay').textContent = '7';
            updateDepositPreview();
            
            // Reload data
            await loadInitialData();
        } else {
            showCustomAlert(result.error, 'Gagal Membuat Deposito', 'error');
        }
    } catch (error) {
        console.error('Failed to create deposit:', error);
        showCustomAlert('Terjadi kesalahan saat membuat deposito. Silakan coba lagi.', 'Error', 'error');
    }
}

// Withdraw Deposit
async function withdrawDeposit(depositId) {
    const confirmed = await showCustomConfirm(
        'Tarik deposito sekarang?\n\nJika belum jatuh tempo, Anda hanya akan menerima pokok tanpa bunga.',
        'Tarik Deposito',
        'info'
    );
    
    if (!confirmed) return;
    
    try {
        const response = await fetch(`${API_BASE}/api/deposits/withdraw/${depositId}`, {
            method: 'POST'
        });
        
        const result = await response.json();
        
        if (result.success) {
            const type = result.isMature ? 'success' : 'warning';
            const title = result.isMature ? 'Deposito Jatuh Tempo' : 'Penarikan Awal';
            const msg = result.isMature 
                ? `Deposito berhasil ditarik!\nJumlah diterima: ${formatCurrency(result.returnAmount)}\n(Pokok + Bunga)`
                : `Deposito ditarik sebelum jatuh tempo.\nJumlah diterima: ${formatCurrency(result.returnAmount)}\n(Hanya pokok, tanpa bunga)`;
            showCustomAlert(msg, title, type);
            
            // Reload data
            await loadInitialData();
        } else {
            showCustomAlert(result.error, 'Gagal Menarik Deposito', 'error');
        }
    } catch (error) {
        console.error('Failed to withdraw deposit:', error);
        showCustomAlert('Terjadi kesalahan saat menarik deposito. Silakan coba lagi.', 'Error', 'error');
    }
}

// Create Loan
async function createLoan() {
    if (loans.length > 0) {
        showCustomAlert('Anda sudah memiliki pinjaman aktif. Selesaikan pinjaman saat ini terlebih dahulu.', 'Pinjaman Aktif', 'warning');
        return;
    }
    
    const confirmed = await showCustomConfirm(
        'Pinjaman Rp 100.000 akan ditambahkan ke saldo Anda.\nPengembalian: Rp 120.000 dalam 30 hari (120 detik per hari game).\n\nSetelah jatuh tempo, uang akan otomatis terpotong dari saldo.',
        'Ajukan Pinjaman?',
        'warning'
    );
    
    if (!confirmed) return;
    
    try {
        const response = await fetch(`${API_BASE}/api/loans/create`, {
            method: 'POST'
        });
        
        const result = await response.json();
        
        if (result.success) {
            const message = `Pinjaman Rp 100.000 berhasil disetujui!\nSaldo Anda bertambah: ${formatCurrency(result.loan.amount)}\n\nJatuh tempo: 30 hari game\nTotal bayar: ${formatCurrency(result.loan.repaymentAmount)}`;
            showCustomAlert(message, 'Pinjaman Disetujui', 'success');
            
            // Reload data
            await loadInitialData();
        } else {
            showCustomAlert(result.error, 'Gagal Mengajukan Pinjaman', 'error');
        }
    } catch (error) {
        console.error('Failed to create loan:', error);
        showCustomAlert('Terjadi kesalahan saat mengajukan pinjaman. Silakan coba lagi.', 'Error', 'error');
    }
}

// Repay Loan
async function repayLoan(loanId) {
    const confirmed = await showCustomConfirm(
        'Anda akan membayar pinjaman sekarang.\nJumlah yang akan terpotong: Rp 120.000\n\nLanjutkan pembayaran?',
        'Bayar Pinjaman',
        'warning'
    );
    
    if (!confirmed) return;
    
    try {
        const response = await fetch(`${API_BASE}/api/loans/repay/${loanId}`, {
            method: 'POST'
        });
        
        const result = await response.json();
        
        if (result.success) {
            const message = `Pinjaman berhasil dibayar!\nJumlah terpotong: ${formatCurrency(result.repaidAmount)}\nSaldo tersisa: ${formatCurrency(result.newBalance)}`;
            showCustomAlert(message, 'Pembayaran Berhasil', 'success');
            
            // Reload data
            await loadInitialData();
        } else {
            showCustomAlert(result.error, 'Gagal Membayar Pinjaman', 'error');
        }
    } catch (error) {
        console.error('Failed to repay loan:', error);
        showCustomAlert('Terjadi kesalahan saat membayar pinjaman. Silakan coba lagi.', 'Error', 'error');
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

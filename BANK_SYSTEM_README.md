# FLASK Bank System - Port 5001

## 📋 Overview
Server bank kedua yang berjalan di port 5001, berjalan bersamaan dengan Trading Server (port 5000) dengan **shared balance** - uang di kedua server terhubung!

## 🏗️ Architecture

### Java Backend Files (Assets/Plugins/Android/)
1. **BankServer.java** - HTTP Server untuk bank (Port 5001)
2. **BankDataManager.java** - Mengelola deposito & pinjaman
3. **TradingServerBridge.java** - Updated untuk start 2 server sekaligus
4. **TradingServer.java** - Updated untuk support shared MoneyManager

### Frontend Files (Assets/StreamingAssets/server/bank/)
1. **index.html** - Main bank page dengan 2 tabs
2. **static/css/bank.css** - Styling (sama style dengan trading)
3. **static/js/bank.js** - Client logic untuk deposit & loan

## ✨ Features

### 💰 Deposito Tab
- **Tenor**: 1-30 hari (slider)
- **Bunga**: 6% per tahun (fixed)
- **Preview**: Real-time calculation sebelum buat deposito
- **Status**: Active/Mature dengan progress bar
- **Penarikan**:
  - Jatuh tempo: Dapat pokok + bunga
  - Sebelum tempo: Hanya dapat pokok (no interest)

### 💳 Peminjaman Tab
- **Jumlah**: Rp 100.000 (fixed)
- **Tenor**: 30 hari (fixed)
- **Bunga**: 20% (fixed)
- **Total Bayar**: Rp 120.000
- **Auto-Deduct**: Setelah 30 hari, otomatis potong dari saldo
- **Limit**: Hanya 1 pinjaman aktif per user

## 🔗 Shared Balance System

### Cara Kerja:
```java
// TradingServerBridge.java
MoneyManager sharedMoneyManager = new MoneyManager(activity);

// Kedua server gunakan instance yang sama
TradingServer tradingServer = new TradingServer(context, path, sharedMoneyManager);
BankServer bankServer = new BankServer(context, sharedMoneyManager);
```

### Benefit:
- ✅ Uang di trading (port 5000) = uang di bank (port 5001)
- ✅ Buy saham → balance berkurang di bank
- ✅ Buat deposito → balance berkurang di trading
- ✅ Dapat pinjaman → balance bertambah di kedua server
- ✅ Real-time sync otomatis

## 📡 API Endpoints

### Balance
- `GET /api/balance` - Get current balance

### Deposits
- `GET /api/deposits` - Get all active deposits
- `POST /api/deposits/create` - Create new deposit
  ```json
  {
    "amount": 100000,
    "days": 7
  }
  ```
- `POST /api/deposits/withdraw/{id}` - Withdraw deposit

### Loans
- `GET /api/loans` - Get all active loans
- `POST /api/loans/create` - Create loan (100k fixed)
- `POST /api/loans/repay/{id}` - Repay loan manually

### Stats
- `GET /api/stats` - Get bank statistics

## 💻 How to Use

### 1. Akses Bank Server
```
http://localhost:5001
atau
http://[YOUR_IP]:5001
```

### 2. Di Unity - Setup Webview
Gunakan CanvasToWebview.cs dengan URL:
```csharp
webviewURL = "http://localhost:5001";
```

### 3. Contoh Penggunaan

#### Buat Deposito:
1. Masukkan jumlah (minimal 10000)
2. Pilih tenor dengan slider (1-30 hari)
3. Lihat preview bunga
4. Klik "Buat Deposito"

#### Ajukan Pinjaman:
1. Pastikan belum ada pinjaman aktif
2. Klik "Ajukan Pinjaman"
3. Uang langsung masuk ke saldo
4. Bayar sebelum 30 hari atau auto-deduct

## 🔄 Background Tasks

### Auto Loan Repayment Checker
```java
// BankDataManager.java - Runs every 60 seconds
Timer timer = new Timer(true);
timer.scheduleAtFixedRate(new TimerTask() {
    @Override
    public void run() {
        checkOverdueLoans();
    }
}, 60000, 60000);
```

**Behavior:**
- Check setiap 1 menit
- Jika loan jatuh tempo (30 hari):
  - Jika saldo >= repayment: Auto potong penuh
  - Jika saldo < repayment: Potong sebisanya, loan tetap cleared
- Log semua activity ke Android logcat

## 📊 Frontend Features

### Tab Navigation
- Smooth transition antara Deposito & Peminjaman
- Active state styling

### Real-time Updates
- Auto refresh setiap 10 detik
- Manual refresh saat action (create/withdraw/repay)

### Responsive Design
- Mobile-friendly
- Sama theme dengan Trading Server
- Card-based layout

### Progress Indicators
- Visual progress bar untuk deposit & loan
- Days remaining countdown
- Status badges (Active/Mature/Overdue)

## 🛠️ Testing

### Test Deposit Flow:
1. Buka http://localhost:5001
2. Check saldo awal
3. Buat deposit Rp 50.000 tenor 7 hari
4. Verify saldo berkurang
5. Check di trading server (port 5000) - saldo sama
6. Wait atau change system date untuk test maturity

### Test Loan Flow:
1. Klik "Ajukan Pinjaman"
2. Verify saldo +100.000
3. Check di trading server - balance updated
4. Bayar manual atau wait 30 hari untuk auto-deduct
5. Verify saldo terpotong 120.000

### Test Cross-Server Balance:
1. Port 5001: Buat deposit 50K → Balance berkurang
2. Port 5000: Buy saham → Balance berkurang  
3. Verify kedua server show same balance
4. Port 5001: Ajukan pinjaman → Balance +100K
5. Port 5000: Check balance increased

## 🐛 Debugging

### Logs to Check:
```java
// Startup
"✅ Trading Server started on port 5000"
"✅ Bank Server started on port 5001"

// Deposit
"Deposit created: DEP00001, Amount: 50000.00, Days: 7"

// Loan
"Loan created: LOAN00001, Amount: 100000.00, Repayment: 120000.00"

// Auto repayment
"Auto-repaid loan: LOAN00001, Amount: 120000.00"
```

### Common Issues:

1. **"Insufficient balance"**
   - Check current balance di /api/balance
   - Pastikan punya cukup uang

2. **"You already have an active loan"**
   - Hanya 1 loan allowed per user
   - Bayar loan existing dulu

3. **Port 5001 not accessible**
   - Verify BankServer started
   - Check firewall settings
   - Check TradingServerBridge logs

## 📝 Code Structure

### BankDataManager.java
```
├── createDeposit() - Validate & create deposit
├── withdrawDeposit() - Handle withdrawal (mature/early)
├── createLoan() - Create fixed 100K loan
├── repayLoan() - Manual repayment
├── checkOverdueLoans() - Auto-deduct background task
├── getActiveDeposits() - List all deposits
├── getActiveLoans() - List all loans
└── getStats() - Summary statistics
```

### bank.js
```
├── loadBalance() - Fetch current balance
├── loadDeposits() - Fetch & render deposits
├── loadLoans() - Fetch & render loans
├── createDeposit() - Submit new deposit
├── withdrawDeposit() - Withdraw deposit
├── createLoan() - Submit loan request
├── repayLoan() - Manual loan repayment
└── startAutoRefresh() - Polling every 10s
```

## 🎨 UI Components

### Deposit Card
- Input jumlah dengan validation
- Range slider untuk tenor (1-30)
- Preview calculation box
- Submit button

### Deposit Item
- ID & status badge
- Principal, interest, total
- Progress bar with percentage
- Withdraw button (conditional text)

### Loan Info Card
- Fixed loan details (100K, 30d, 20%)
- Warning message about auto-deduct
- Submit button

### Loan Item
- ID & status badge
- Amount, interest, total repayment
- Progress bar with days remaining
- Repay button or overdue warning

## 🚀 Deployment

Sudah terintegrasi! Tinggal:
1. Build APK dari Unity
2. Install di Android
3. Server auto-start bersamaan
4. Port 5000 = Trading
5. Port 5001 = Bank
6. Balance shared otomatis

## 📚 Next Steps

Untuk extend functionality:
- Add loan history
- Add deposit history
- Multiple loan tiers
- Variable interest rates
- Early repayment discount
- Loan approval workflow
- Credit score system
- Notification system for maturity/overdue

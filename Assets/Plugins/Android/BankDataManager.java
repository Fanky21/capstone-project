package com.trading.localserver;

import android.content.Context;
import org.json.JSONObject;
import org.json.JSONArray;
import org.json.JSONException;
import java.text.SimpleDateFormat;
import java.util.*;

/**
 * Manages deposits and loans
 */
public class BankDataManager {
    private Context context;
    private MoneyManager moneyManager;
    
    private ArrayList<Deposit> activeDeposits;
    private ArrayList<Loan> activeLoans;
    private int nextDepositId = 1;
    private int nextLoanId = 1;
    
    // Constants
    private static final double DEPOSIT_INTEREST_RATE = 0.06; // 6% per year
    private static final double LOAN_AMOUNT = 100000.0;
    private static final int LOAN_DURATION_DAYS = 30;
    private static final double LOAN_INTEREST_RATE = 0.20; // 20%
    private static final long SECONDS_PER_DAY = 120; // 1 game day = 120 real seconds (2 minutes)
    
    public BankDataManager(Context context, MoneyManager moneyManager) {
        this.context = context;
        this.moneyManager = moneyManager;
        this.activeDeposits = new ArrayList<>();
        this.activeLoans = new ArrayList<>();
        
        // Start background checker for auto loan repayment
        startAutoRepaymentChecker();
        
        android.util.Log.i("BankDataManager", "Initialized");
    }
    
    /**
     * Create new deposit
     */
    public JSONObject createDeposit(double amount, int days) throws JSONException {
        JSONObject response = new JSONObject();
        
        // Validate
        if (amount <= 0) {
            response.put("success", false);
            response.put("error", "Invalid amount");
            return response;
        }
        
        if (days < 1 || days > 30) {
            response.put("success", false);
            response.put("error", "Tenor must be between 1-30 days");
            return response;
        }
        
        if (moneyManager.getCash() < amount) {
            response.put("success", false);
            response.put("error", "Insufficient balance");
            return response;
        }
        
        // Deduct money
        moneyManager.subtractCash(amount);
        
        // Calculate maturity using real-time seconds (120 seconds per game day)
        long startTimestamp = System.currentTimeMillis();
        long durationSeconds = days * SECONDS_PER_DAY;
        
        // Calculate interest (simple interest for days)
        double dailyRate = DEPOSIT_INTEREST_RATE / 365;
        double interest = amount * dailyRate * days;
        double totalReturn = amount + interest;
        
        // Create deposit
        Deposit deposit = new Deposit();
        deposit.id = "DEP" + String.format("%05d", nextDepositId++);
        deposit.amount = amount;
        deposit.days = days;
        deposit.interest = interest;
        deposit.totalReturn = totalReturn;
        deposit.startTimestamp = startTimestamp;
        deposit.durationSeconds = durationSeconds;
        deposit.status = "active";
        
        activeDeposits.add(deposit);
        
        android.util.Log.i("BankDataManager", String.format(
            "Deposit created: %s, Amount: %.2f, Days: %d, Interest: %.2f",
            deposit.id, amount, days, interest));
        
        response.put("success", true);
        response.put("message", "Deposit created successfully");
        response.put("deposit", depositToJson(deposit));
        response.put("newBalance", moneyManager.getCash());
        
        return response;
    }
    
    /**
     * Withdraw deposit (before or at maturity)
     */
    public JSONObject withdrawDeposit(String depositId) throws JSONException {
        JSONObject response = new JSONObject();
        
        Deposit deposit = findDeposit(depositId);
        if (deposit == null) {
            response.put("success", false);
            response.put("error", "Deposit not found");
            return response;
        }
        
        // Check if mature (based on elapsed real-time seconds)
        long currentTimestamp = System.currentTimeMillis();
        long elapsedMillis = currentTimestamp - deposit.startTimestamp;
        long elapsedSeconds = elapsedMillis / 1000;
        boolean isMature = elapsedSeconds >= deposit.durationSeconds;
        
        double returnAmount;
        if (isMature) {
            // Full return with interest
            returnAmount = deposit.totalReturn;
        } else {
            // Early withdrawal - no interest
            returnAmount = deposit.amount;
        }
        
        // Add money back
        moneyManager.addCash(returnAmount);
        
        // Remove deposit
        activeDeposits.remove(deposit);
        
        android.util.Log.i("BankDataManager", String.format(
            "Deposit withdrawn: %s, Returned: %.2f, Mature: %b",
            depositId, returnAmount, isMature));
        
        response.put("success", true);
        response.put("message", isMature ? "Deposit matured" : "Early withdrawal (no interest)");
        response.put("returnAmount", returnAmount);
        response.put("isMature", isMature);
        response.put("newBalance", moneyManager.getCash());
        
        return response;
    }
    
    /**
     * Create loan (fixed 100k, 30 days)
     */
    public JSONObject createLoan() throws JSONException {
        JSONObject response = new JSONObject();
        
        // Check if user already has active loan
        if (!activeLoans.isEmpty()) {
            response.put("success", false);
            response.put("error", "You already have an active loan");
            return response;
        }
        
        // Calculate due date using real-time seconds (120 seconds per game day)
        long startTimestamp = System.currentTimeMillis();
        long durationSeconds = LOAN_DURATION_DAYS * SECONDS_PER_DAY;
        
        // Calculate repayment amount (principal + 20% interest)
        double repaymentAmount = LOAN_AMOUNT * (1 + LOAN_INTEREST_RATE);
        
        // Create loan
        Loan loan = new Loan();
        loan.id = "LOAN" + String.format("%05d", nextLoanId++);
        loan.amount = LOAN_AMOUNT;
        loan.interestRate = LOAN_INTEREST_RATE;
        loan.repaymentAmount = repaymentAmount;
        loan.startTimestamp = startTimestamp;
        loan.durationSeconds = durationSeconds;
        loan.status = "active";
        
        activeLoans.add(loan);
        
        // Add loan money to balance
        moneyManager.addCash(LOAN_AMOUNT);
        
        android.util.Log.i("BankDataManager", String.format(
            "Loan created: %s, Amount: %.2f, Repayment: %.2f",
            loan.id, LOAN_AMOUNT, repaymentAmount));
        
        response.put("success", true);
        response.put("message", "Loan approved");
        response.put("loan", loanToJson(loan));
        response.put("newBalance", moneyManager.getCash());
        
        return response;
    }
    
    /**
     * Repay loan manually
     */
    public JSONObject repayLoan(String loanId) throws JSONException {
        JSONObject response = new JSONObject();
        
        Loan loan = findLoan(loanId);
        if (loan == null) {
            response.put("success", false);
            response.put("error", "Loan not found");
            return response;
        }
        
        // Check if user has enough money
        if (moneyManager.getCash() < loan.repaymentAmount) {
            response.put("success", false);
            response.put("error", "Insufficient balance to repay loan");
            response.put("required", loan.repaymentAmount);
            response.put("current", moneyManager.getCash());
            return response;
        }
        
        // Deduct repayment amount
        moneyManager.subtractCash(loan.repaymentAmount);
        
        // Remove loan
        activeLoans.remove(loan);
        
        android.util.Log.i("BankDataManager", String.format(
            "Loan repaid: %s, Amount: %.2f", loanId, loan.repaymentAmount));
        
        response.put("success", true);
        response.put("message", "Loan repaid successfully");
        response.put("repaidAmount", loan.repaymentAmount);
        response.put("newBalance", moneyManager.getCash());
        
        return response;
    }
    
    /**
     * Auto repayment checker (runs every minute)
     */
    private void startAutoRepaymentChecker() {
        Timer timer = new Timer(true);
        timer.scheduleAtFixedRate(new TimerTask() {
            @Override
            public void run() {
                checkOverdueLoans();
            }
        }, 60000, 60000); // Check every minute
    }
    
    /**
     * Check and auto-deduct overdue loans
     */
    private void checkOverdueLoans() {
        long currentTimestamp = System.currentTimeMillis();
        List<Loan> toRemove = new ArrayList<>();
        
        for (Loan loan : activeLoans) {
            long elapsedMillis = currentTimestamp - loan.startTimestamp;
            long elapsedSeconds = elapsedMillis / 1000;
            
            if (elapsedSeconds >= loan.durationSeconds) {
                // Loan is overdue - auto deduct
                double currentBalance = moneyManager.getCash();
                
                if (currentBalance >= loan.repaymentAmount) {
                    // Sufficient balance - deduct
                    moneyManager.subtractCash(loan.repaymentAmount);
                    toRemove.add(loan);
                    
                    android.util.Log.i("BankDataManager", String.format(
                        "Auto-repaid loan: %s, Amount: %.2f", loan.id, loan.repaymentAmount));
                } else {
                    // Insufficient - deduct whatever is available
                    moneyManager.subtractCash(currentBalance);
                    toRemove.add(loan);
                    
                    android.util.Log.w("BankDataManager", String.format(
                        "Partial auto-repay loan: %s, Deducted: %.2f (Required: %.2f)",
                        loan.id, currentBalance, loan.repaymentAmount));
                }
            }
        }
        
        activeLoans.removeAll(toRemove);
    }
    
    /**
     * Get active deposits
     */
    public JSONArray getActiveDeposits() throws JSONException {
        JSONArray array = new JSONArray();
        for (Deposit deposit : activeDeposits) {
            array.put(depositToJson(deposit));
        }
        return array;
    }
    
    /**
     * Get active loans
     */
    public JSONArray getActiveLoans() throws JSONException {
        JSONArray array = new JSONArray();
        for (Loan loan : activeLoans) {
            array.put(loanToJson(loan));
        }
        return array;
    }
    
    /**
     * Get statistics
     */
    public JSONObject getStats() throws JSONException {
        JSONObject stats = new JSONObject();
        
        double totalDeposited = 0;
        double totalDepositReturns = 0;
        for (Deposit d : activeDeposits) {
            totalDeposited += d.amount;
            totalDepositReturns += d.totalReturn;
        }
        
        double totalLoanAmount = 0;
        double totalLoanRepayment = 0;
        for (Loan l : activeLoans) {
            totalLoanAmount += l.amount;
            totalLoanRepayment += l.repaymentAmount;
        }
        
        stats.put("activeDeposits", activeDeposits.size());
        stats.put("totalDeposited", totalDeposited);
        stats.put("expectedDepositReturns", totalDepositReturns);
        stats.put("activeLoans", activeLoans.size());
        stats.put("totalLoanAmount", totalLoanAmount);
        stats.put("totalLoanRepayment", totalLoanRepayment);
        stats.put("currentBalance", moneyManager.getCash());
        
        return stats;
    }
    
    private Deposit findDeposit(String id) {
        for (Deposit d : activeDeposits) {
            if (d.id.equals(id)) return d;
        }
        return null;
    }
    
    private Loan findLoan(String id) {
        for (Loan l : activeLoans) {
            if (l.id.equals(id)) return l;
        }
        return null;
    }
    
    private JSONObject depositToJson(Deposit d) throws JSONException {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.US);
        
        JSONObject json = new JSONObject();
        json.put("id", d.id);
        json.put("amount", d.amount);
        json.put("days", d.days);
        json.put("interest", d.interest);
        json.put("totalReturn", d.totalReturn);
        
        // Calculate elapsed time
        long currentTimestamp = System.currentTimeMillis();
        long elapsedMillis = currentTimestamp - d.startTimestamp;
        long elapsedSeconds = elapsedMillis / 1000;
        long remainingSeconds = Math.max(0, d.durationSeconds - elapsedSeconds);
        
        // Convert timestamps to dates for display
        Date startDate = new Date(d.startTimestamp);
        Date maturityDate = new Date(d.startTimestamp + (d.durationSeconds * 1000));
        
        json.put("startDate", sdf.format(startDate));
        json.put("maturityDate", sdf.format(maturityDate));
        json.put("status", d.status);
        json.put("elapsedSeconds", elapsedSeconds);
        json.put("remainingSeconds", remainingSeconds);
        json.put("daysRemaining", (int)(remainingSeconds / SECONDS_PER_DAY));
        json.put("isMature", remainingSeconds == 0);
        
        return json;
    }
    
    private JSONObject loanToJson(Loan l) throws JSONException {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.US);
        
        JSONObject json = new JSONObject();
        json.put("id", l.id);
        json.put("amount", l.amount);
        json.put("interestRate", l.interestRate);
        json.put("repaymentAmount", l.repaymentAmount);
        
        // Calculate elapsed time
        long currentTimestamp = System.currentTimeMillis();
        long elapsedMillis = currentTimestamp - l.startTimestamp;
        long elapsedSeconds = elapsedMillis / 1000;
        long remainingSeconds = Math.max(0, l.durationSeconds - elapsedSeconds);
        
        // Convert timestamps to dates for display
        Date startDate = new Date(l.startTimestamp);
        Date dueDate = new Date(l.startTimestamp + (l.durationSeconds * 1000));
        
        json.put("startDate", sdf.format(startDate));
        json.put("dueDate", sdf.format(dueDate));
        json.put("status", l.status);
        json.put("elapsedSeconds", elapsedSeconds);
        json.put("remainingSeconds", remainingSeconds);
        json.put("daysRemaining", (int)(remainingSeconds / SECONDS_PER_DAY));
        json.put("isOverdue", remainingSeconds == 0);
        
        return json;
    }
    
    // Data classes
    private static class Deposit {
        String id;
        double amount;
        int days;
        double interest;
        double totalReturn;
        long startTimestamp;  // milliseconds
        long durationSeconds; // total duration in real seconds
        String status;
    }
    
    private static class Loan {
        String id;
        double amount;
        double interestRate;
        double repaymentAmount;
        long startTimestamp;  // milliseconds
        long durationSeconds; // total duration in real seconds
        String status;
    }
}

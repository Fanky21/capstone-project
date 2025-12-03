package com.trading.localserver;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.HashMap;
import java.util.Map;

/**
 * Money and Portfolio Manager for Android
 * Manages global cash and portfolio holdings
 */
public class MoneyManager {
    
    private double globalCash;
    private Map<String, PortfolioItem> portfolio;
    
    public MoneyManager() {
        this.globalCash = 1000000.0; // Default starting cash: 1 million
        this.portfolio = new HashMap<>();
    }
    
    /**
     * Get current cash
     */
    public double getCash() {
        return globalCash;
    }
    
    /**
     * Set cash to specific amount
     */
    public void setCash(double amount) {
        this.globalCash = amount;
    }
    
    /**
     * Add cash
     */
    public void addCash(double amount) {
        this.globalCash += amount;
    }
    
    /**
     * Subtract cash
     */
    public void subtractCash(double amount) {
        this.globalCash -= amount;
    }
    
    /**
     * Buy stock
     */
    public void buyStock(String ticker, int quantity, double price) {
        double totalCost = price * quantity;
        subtractCash(totalCost);
        
        PortfolioItem item = portfolio.get(ticker);
        if (item == null) {
            // New position
            item = new PortfolioItem(ticker, quantity, price);
            portfolio.put(ticker, item);
        } else {
            // Add to existing position - calculate new average price
            int totalQuantity = item.quantity + quantity;
            double totalValue = (item.avgPrice * item.quantity) + (price * quantity);
            item.quantity = totalQuantity;
            item.avgPrice = totalValue / totalQuantity;
        }
    }
    
    /**
     * Sell stock
     */
    public void sellStock(String ticker, int quantity, double price) {
        PortfolioItem item = portfolio.get(ticker);
        if (item != null) {
            item.quantity -= quantity;
            if (item.quantity <= 0) {
                portfolio.remove(ticker);
            }
        }
        
        double totalRevenue = price * quantity;
        addCash(totalRevenue);
    }
    
    /**
     * Check if has sufficient stock
     */
    public boolean hasStock(String ticker, int quantity) {
        PortfolioItem item = portfolio.get(ticker);
        return item != null && item.quantity >= quantity;
    }
    
    /**
     * Get stock quantity owned
     */
    public int getStockQuantity(String ticker) {
        PortfolioItem item = portfolio.get(ticker);
        return item != null ? item.quantity : 0;
    }
    
    /**
     * Get portfolio as JSON (Flask-compatible format)
     */
    public JSONObject getPortfolioJson(CompanyDataManager companyDataManager) throws JSONException {
        JSONArray portfolioArray = new JSONArray();
        double totalValue = globalCash;
        double totalProfit = 0;
        
        for (PortfolioItem item : portfolio.values()) {
            JSONObject company = companyDataManager.getCompany(item.ticker);
            
            if (company != null) {
                double currentPrice = company.getDouble("currentPrice");
                double stockValue = currentPrice * item.quantity;
                double profit = (currentPrice - item.avgPrice) * item.quantity;
                double profitPercent = ((currentPrice - item.avgPrice) / item.avgPrice) * 100;
                
                JSONObject portfolioItem = new JSONObject();
                portfolioItem.put("ticker", item.ticker);
                portfolioItem.put("name", company.getString("name"));
                portfolioItem.put("shares", item.quantity); // Flask uses "shares" not "quantity"
                portfolioItem.put("avgPrice", item.avgPrice);
                portfolioItem.put("currentPrice", currentPrice);
                portfolioItem.put("value", stockValue);
                portfolioItem.put("profit", profit);
                portfolioItem.put("profitPercent", profitPercent);
                
                portfolioArray.put(portfolioItem);
                totalValue += stockValue;
                totalProfit += profit;
            }
        }
        
        JSONObject result = new JSONObject();
        result.put("cash", globalCash);
        result.put("portfolio", portfolioArray);
        result.put("totalValue", totalValue);
        result.put("totalProfit", totalProfit);
        
        android.util.Log.d("MoneyManager", "Portfolio: " + portfolioArray.length() + " items, Cash: Rp" + globalCash);
        
        return result;
    }
    
    /**
     * Reset portfolio and cash
     */
    public void resetPortfolio() {
        this.globalCash = 1000000.0;
        this.portfolio.clear();
    }
    
    /**
     * Portfolio item class
     */
    private static class PortfolioItem {
        String ticker;
        int quantity;
        double avgPrice;
        
        PortfolioItem(String ticker, int quantity, double avgPrice) {
            this.ticker = ticker;
            this.quantity = quantity;
            this.avgPrice = avgPrice;
        }
    }
}

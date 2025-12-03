import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Money and Portfolio Manager
 * Manages global cash and portfolio holdings
 */
public class MoneyManager {
    
    private double globalCash;
    private Map<String, PortfolioItem> portfolio;
    
    public MoneyManager() {
        this.globalCash = 1.0; // Default starting cash
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
     * Buy shares
     */
    public void buyShares(String ticker, int shares, double currentPrice) {
        PortfolioItem item = portfolio.get(ticker);
        
        if (item == null) {
            // New position
            item = new PortfolioItem(ticker, shares, currentPrice);
            portfolio.put(ticker, item);
        } else {
            // Add to existing position
            int totalShares = item.shares + shares;
            double totalCost = (item.avgPrice * item.shares) + (currentPrice * shares);
            item.shares = totalShares;
            item.avgPrice = totalCost / totalShares;
        }
    }
    
    /**
     * Sell shares
     */
    public void sellShares(String ticker, int shares) {
        PortfolioItem item = portfolio.get(ticker);
        
        if (item != null) {
            item.shares -= shares;
            if (item.shares <= 0) {
                portfolio.remove(ticker);
            }
        }
    }
    
    /**
     * Check if has sufficient shares
     */
    public boolean hasShares(String ticker, int shares) {
        PortfolioItem item = portfolio.get(ticker);
        return item != null && item.shares >= shares;
    }
    
    /**
     * Get portfolio as JSON array
     */
    public JSONArray getPortfolio() throws JSONException {
        JSONArray result = new JSONArray();
        
        for (PortfolioItem item : portfolio.values()) {
            JSONObject obj = new JSONObject();
            obj.put("ticker", item.ticker);
            obj.put("shares", item.shares);
            obj.put("avgPrice", item.avgPrice);
            result.put(obj);
        }
        
        return result;
    }
    
    /**
     * Get portfolio with details (includes current prices and profit)
     */
    public JSONObject getPortfolioWithDetails(CompanyDataManager companyDataManager) throws JSONException {
        JSONArray portfolioWithDetails = new JSONArray();
        double totalValue = globalCash;
        
        for (PortfolioItem item : portfolio.values()) {
            JSONObject company = companyDataManager.getCompany(item.ticker);
            
            if (company != null) {
                double currentPrice = company.getDouble("currentPrice");
                double profit = (currentPrice - item.avgPrice) * item.shares;
                double profitPercent = ((currentPrice - item.avgPrice) / item.avgPrice) * 100;
                
                JSONObject portfolioItem = new JSONObject();
                portfolioItem.put("ticker", item.ticker);
                portfolioItem.put("name", company.getString("name"));
                portfolioItem.put("shares", item.shares);
                portfolioItem.put("avgPrice", item.avgPrice);
                portfolioItem.put("currentPrice", currentPrice);
                portfolioItem.put("profit", profit);
                portfolioItem.put("profitPercent", profitPercent);
                
                portfolioWithDetails.put(portfolioItem);
                totalValue += currentPrice * item.shares;
            }
        }
        
        JSONObject result = new JSONObject();
        result.put("cash", globalCash);
        result.put("portfolio", portfolioWithDetails);
        result.put("totalValue", totalValue);
        
        return result;
    }
    
    /**
     * Reset portfolio and cash
     */
    public void reset() {
        this.globalCash = 1.0;
        this.portfolio.clear();
    }
    
    /**
     * Portfolio item class
     */
    private static class PortfolioItem {
        String ticker;
        int shares;
        double avgPrice;
        
        PortfolioItem(String ticker, int shares, double avgPrice) {
            this.ticker = ticker;
            this.shares = shares;
            this.avgPrice = avgPrice;
        }
    }
}

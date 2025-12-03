package com.trading.localserver;

import android.content.Context;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.*;
import java.util.Random;

/**
 * Company Data Manager for Android
 * Manages company data, prices, and market statistics from StreamingAssets
 */
public class CompanyDataManager {
    
    private Context context;
    private JSONArray companies;
    private Random random;
    
    public CompanyDataManager(Context context) {
        this.context = context;
        this.random = new Random();
        loadCompanies();
    }
    
    /**
     * Load companies from assets
     */
    private void loadCompanies() {
        try {
            InputStream is = context.getAssets().open("server/companies.json");
            String content = readStream(is);
            companies = new JSONArray(content);
            
            // Initialize missing fields for each company
            for (int i = 0; i < companies.length(); i++) {
                JSONObject company = companies.getJSONObject(i);
                
                // Ensure all price fields exist
                if (!company.has("currentPrice")) {
                    company.put("currentPrice", company.getDouble("price"));
                }
                if (!company.has("previousPrice")) {
                    company.put("previousPrice", company.getDouble("price"));
                }
                if (!company.has("change")) {
                    company.put("change", 0.0);
                }
                if (!company.has("changePercent")) {
                    company.put("changePercent", 0.0);
                }
                
                // Initialize priceHistory if not exists
                if (!company.has("priceHistory")) {
                    JSONArray history = new JSONArray();
                    double price = company.getDouble("price");
                    // Fill with current price (7 days)
                    for (int j = 0; j < 7; j++) {
                        history.put(price);
                    }
                    company.put("priceHistory", history);
                }
            }
            
            android.util.Log.i("CompanyDataManager", "Loaded " + companies.length() + " companies from assets");
        } catch (IOException | JSONException e) {
            android.util.Log.e("CompanyDataManager", "Failed to load companies from assets", e);
            companies = new JSONArray();
        }
    }
    
    /**
     * Read stream to string
     */
    private String readStream(InputStream is) throws IOException {
        BufferedReader reader = new BufferedReader(new InputStreamReader(is, "UTF-8"));
        StringBuilder sb = new StringBuilder();
        String line;
        while ((line = reader.readLine()) != null) {
            sb.append(line);
        }
        reader.close();
        return sb.toString();
    }
    
    /**
     * Get all companies
     */
    public JSONArray getAllCompanies() {
        return companies;
    }
    
    /**
     * Get company by ticker
     */
    public JSONObject getCompany(String ticker) {
        try {
            for (int i = 0; i < companies.length(); i++) {
                JSONObject company = companies.getJSONObject(i);
                if (company.getString("ticker").equalsIgnoreCase(ticker)) {
                    return company;
                }
            }
        } catch (JSONException e) {
            android.util.Log.e("CompanyDataManager", "Error getting company", e);
        }
        return null;
    }
    
    /**
     * Update all stock prices (simulate market movement)
     */
    public void updateAllPrices() {
        try {
            for (int i = 0; i < companies.length(); i++) {
                JSONObject company = companies.getJSONObject(i);
                
                // Store previous price
                double currentPrice = company.getDouble("price");
                company.put("previousPrice", currentPrice);
                
                // Random price change (-5% to +5%)
                double changePercent = (random.nextDouble() - 0.5) * 0.1;
                double newPrice = currentPrice * (1 + changePercent);
                newPrice = Math.max(newPrice, 1.0); // Minimum $1
                newPrice = Math.round(newPrice * 100.0) / 100.0; // Round to 2 decimals
                
                company.put("price", newPrice);
                
                // Update currentPrice field too (compatibility)
                company.put("currentPrice", newPrice);
                
                // Update price history (keep last 7 days)
                JSONArray history = company.optJSONArray("priceHistory");
                if (history == null) {
                    history = new JSONArray();
                }
                history.put(newPrice);
                if (history.length() > 7) {
                    // Remove oldest (first) element
                    JSONArray newHistory = new JSONArray();
                    for (int j = 1; j < history.length(); j++) {
                        newHistory.put(history.get(j));
                    }
                    history = newHistory;
                }
                company.put("priceHistory", history);
                
                // Calculate change
                double change = newPrice - currentPrice;
                double changePercentActual = (change / currentPrice) * 100;
                company.put("change", change);
                company.put("changePercent", changePercentActual);
            }
            
            android.util.Log.d("CompanyDataManager", "Updated prices for " + companies.length() + " companies");
        } catch (JSONException e) {
            android.util.Log.e("CompanyDataManager", "Error updating prices", e);
        }
    }
    
    /**
     * Get market statistics
     */
    public JSONObject getMarketStats() throws JSONException {
        int totalCompanies = companies.length();
        int gainers = 0;
        int losers = 0;
        double totalChange = 0;
        
        for (int i = 0; i < companies.length(); i++) {
            JSONObject company = companies.getJSONObject(i);
            double change = company.optDouble("change", 0);
            
            if (change > 0) gainers++;
            else if (change < 0) losers++;
            
            totalChange += change;
        }
        
        JSONObject stats = new JSONObject();
        stats.put("totalCompanies", totalCompanies);
        stats.put("gainers", gainers);
        stats.put("losers", losers);
        stats.put("unchanged", totalCompanies - gainers - losers);
        stats.put("avgChange", totalChange / totalCompanies);
        
        return stats;
    }
}

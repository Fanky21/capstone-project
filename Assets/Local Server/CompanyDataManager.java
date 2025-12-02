import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Random;

/**
 * Company Data Manager
 * Manages company data, prices, and market statistics
 */
public class CompanyDataManager {
    
    private String filePath;
    private JSONArray companies;
    private Random random;
    
    public CompanyDataManager(String filePath) {
        this.filePath = filePath;
        this.random = new Random();
        loadCompanies();
    }
    
    /**
     * Load companies from JSON file
     */
    private void loadCompanies() {
        try {
            String content = new String(Files.readAllBytes(Paths.get(filePath)), StandardCharsets.UTF_8);
            companies = new JSONArray(content);
            System.out.println("Loaded " + companies.length() + " companies");
        } catch (IOException | JSONException e) {
            System.err.println("Failed to load companies: " + e.getMessage());
            companies = new JSONArray();
        }
    }
    
    /**
     * Save companies to JSON file
     */
    private void saveCompanies() {
        try {
            Files.write(Paths.get(filePath), companies.toString(2).getBytes(StandardCharsets.UTF_8));
        } catch (IOException | JSONException e) {
            System.err.println("Failed to save companies: " + e.getMessage());
        }
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
            System.err.println("Error getting company: " + e.getMessage());
        }
        return null;
    }
    
    /**
     * Update all stock prices (simulate market movement)
     */
    public void updatePrices() {
        try {
            for (int i = 0; i < companies.length(); i++) {
                JSONObject company = companies.getJSONObject(i);
                
                // Store previous price
                double currentPrice = company.getDouble("currentPrice");
                company.put("previousPrice", currentPrice);
                
                // Random price change (-5% to +5%)
                double changePercent = (random.nextDouble() - 0.5) * 0.1;
                double newPrice = currentPrice * (1 + changePercent);
                newPrice = Math.max(newPrice, 1.0); // Minimum $1
                newPrice = Math.round(newPrice * 100.0) / 100.0; // Round to 2 decimals
                
                company.put("currentPrice", newPrice);
                
                // Update price history
                JSONArray priceHistory = company.getJSONArray("priceHistory");
                if (priceHistory.length() >= 7) {
                    // Remove first element (shift left)
                    JSONArray newHistory = new JSONArray();
                    for (int j = 1; j < priceHistory.length(); j++) {
                        newHistory.put(priceHistory.get(j));
                    }
                    newHistory.put(newPrice);
                    company.put("priceHistory", newHistory);
                } else {
                    priceHistory.put(newPrice);
                }
            }
            
            // Save updated prices
            saveCompanies();
            
        } catch (JSONException e) {
            System.err.println("Error updating prices: " + e.getMessage());
        }
    }
    
    /**
     * Get market statistics
     */
    public JSONObject getMarketStats() throws JSONException {
        int totalCompanies = companies.length();
        int gainers = 0;
        int losers = 0;
        
        for (int i = 0; i < companies.length(); i++) {
            JSONObject company = companies.getJSONObject(i);
            double currentPrice = company.getDouble("currentPrice");
            double previousPrice = company.optDouble("previousPrice", currentPrice);
            
            if (currentPrice > previousPrice) {
                gainers++;
            } else if (currentPrice < previousPrice) {
                losers++;
            }
        }
        
        int unchanged = totalCompanies - gainers - losers;
        
        JSONObject stats = new JSONObject();
        stats.put("total_companies", totalCompanies);
        stats.put("gainers", gainers);
        stats.put("losers", losers);
        stats.put("unchanged", unchanged);
        
        return stats;
    }
}

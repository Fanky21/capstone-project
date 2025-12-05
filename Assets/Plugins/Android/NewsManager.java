package com.trading.localserver;

import android.content.Context;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.*;
import java.util.Random;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

/**
 * News Manager for Android (Flask-style: smart templates only)
 * NO API calls to avoid timeout issues
 */
public class NewsManager {
    
    private Context context;
    private JSONArray activeNews;
    private JSONArray oldNews;
    private Random random;
    private int nextNewsId;
    
    // Stock market related images from Unsplash
    private static final String[] NEWS_IMAGES = {
        "https://images.unsplash.com/photo-1611974789855-9c2a0a7236a3?w=800", // Stock charts
        "https://images.unsplash.com/photo-1590283603385-17ffb3a7f29f?w=800", // Trading floor
        "https://images.unsplash.com/photo-1460925895917-afdab827c52f?w=800", // Analytics
        "https://images.unsplash.com/photo-1642790106117-e829e14a795f?w=800", // Business meeting
        "https://images.unsplash.com/photo-1535320903710-d993d3d77d29?w=800", // Stock market display
        "https://images.unsplash.com/photo-1579532537598-459ecdaf39cc?w=800"  // Financial data
    };
    
    // Advanced smart news templates (like Flask news_generator.py)
    private static final String[][] NEWS_PATTERNS = {
        // Surge (>5%)
        {
            "{company} melonjak spektakuler {percent}%, investor berburu",
            "Rally menguat: {company} melesat {percent}%",
            "Breakout signifikan {company} naik {percent}% ke Rp{price}",
            "Momentum bullish kuat dorong {company} plus {percent}%",
            "Spektakuler! {company} cetak gain {percent}% hari ini"
        },
        // Rise (2-5%)
        {
            "{company} menguat solid {percent}%, sentimen optimis",
            "Momentum positif dorong {company} naik {percent}%",
            "{company} mencatat kenaikan impresif {percent}%",
            "Outlook cerah bawa {company} plus {percent}%",
            "Fundamental mendukung {company} naik {percent}%"
        },
        // Fall (<-5%)
        {
            "{company} hadapi tekanan berat, turun {percent}%",
            "Alert: {company} koreksi dalam {percent}%",
            "Profit taking masif tekan {company} minus {percent}%",
            "Bearish signal: {company} jebol support {percent}%",
            "Volatilitas tinggi, {company} terkoreksi {percent}%"
        },
        // Decline (-2 to -5%)
        {
            "{company} alami koreksi wajar {percent}%",
            "Konsolidasi sehat: {company} melemah {percent}%",
            "Profit taking tekan {company} turun {percent}%",
            "Retracement teknikal bawa {company} minus {percent}%",
            "{company} pullback normal, turun {percent}%"
        },
        // Flat (-2 to 2%)
        {
            "{company} konsolidasi ketat, bergerak {percent}%",
            "Sideways pattern dominasi trading {company}",
            "Range-bound: {company} stabil ±{percent}%",
            "{company} akumulasi diam-diam, naik {percent}%",
            "Balance supply-demand, {company} {percent}%"
        }
    };
    
    public NewsManager(Context context) {
        this.context = context;
        this.random = new Random();
        this.nextNewsId = 1000;
        loadNews();
    }
    
    /**
     * Load news from assets
     */
    private void loadNews() {
        try {
            InputStream is = context.getAssets().open("server/news.json");
            String content = readStream(is);
            
            JSONArray allNewsArray = new JSONArray(content);
            
            activeNews = new JSONArray();
            for (int i = 0; i < Math.min(10, allNewsArray.length()); i++) {
                activeNews.put(allNewsArray.getJSONObject(i));
            }
            
            oldNews = new JSONArray();
            for (int i = 10; i < allNewsArray.length(); i++) {
                oldNews.put(allNewsArray.getJSONObject(i));
            }
            
            android.util.Log.i("NewsManager", "Loaded " + activeNews.length() + 
                " active news and " + oldNews.length() + " old news");
        } catch (IOException | JSONException e) {
            android.util.Log.e("NewsManager", "Failed to load news", e);
            activeNews = new JSONArray();
            oldNews = new JSONArray();
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
     * Get active news
     */
    public JSONArray getActiveNews() {
        return activeNews;
    }
    
    /**
     * Get old news
     */
    public JSONArray getOldNews() {
        return oldNews;
    }
    
    /**
     * Generate new news article (Flask-style: template only, no API)
     */
    public void generateNews(CompanyDataManager companyDataManager) {
        try {
            JSONArray companies = companyDataManager.getAllCompanies();
            if (companies.length() == 0) {
                return;
            }
            
            // Pick random company
            int companyIndex = random.nextInt(companies.length());
            JSONObject company = companies.getJSONObject(companyIndex);
            String companyName = company.getString("name");
            String ticker = company.getString("ticker");
            double currentPrice = company.getDouble("currentPrice");
            double previousPrice = company.optDouble("previousPrice", currentPrice);
            
            // Calculate change
            double change = ((currentPrice - previousPrice) / previousPrice) * 100;
            
            // Generate smart title (NO API, templates only)
            String title = generateSmartTitle(companyName, ticker, change, currentPrice);
            
            // Create timestamp
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.US);
            String date = sdf.format(new Date());
            
            // Generate content
            String content = generateContent(companyName, ticker, change, currentPrice);
            
            // Create summary (first 100 chars of content)
            String summary = content.length() > 100 ? content.substring(0, 100) + "..." : content;
            
            // Create news object
            JSONObject newsItem = new JSONObject();
            newsItem.put("id", nextNewsId++);
            newsItem.put("title", title);
            newsItem.put("date", date);
            newsItem.put("category", "Market Analysis");
            newsItem.put("content", content);
            newsItem.put("summary", summary); // Add summary field for frontend
            newsItem.put("impact", change > 0 ? "positive" : (change < -2 ? "negative" : "neutral"));
            newsItem.put("image", NEWS_IMAGES[random.nextInt(NEWS_IMAGES.length)]); // Random stock market image
            
            // Move oldest active news to old news
            if (activeNews.length() >= 10) {
                JSONObject oldestNews = activeNews.getJSONObject(activeNews.length() - 1);
                oldNews.put(oldestNews);
            }
            
            // Add new news at the beginning
            JSONArray newActiveNews = new JSONArray();
            newActiveNews.put(newsItem);
            for (int i = 0; i < Math.min(9, activeNews.length()); i++) {
                newActiveNews.put(activeNews.getJSONObject(i));
            }
            activeNews = newActiveNews;
            
            android.util.Log.i("NewsManager", "News generated: " + title);
            
        } catch (JSONException e) {
            android.util.Log.e("NewsManager", "Error generating news", e);
        }
    }
    
    /**
     * Generate smart title (Flask-style patterns, NO API)
     */
    private String generateSmartTitle(String companyName, String ticker, double change, double price) {
        String[] templates;
        
        if (change > 5) {
            templates = NEWS_PATTERNS[0]; // Surge
        } else if (change > 2) {
            templates = NEWS_PATTERNS[1]; // Rise
        } else if (change < -5) {
            templates = NEWS_PATTERNS[2]; // Fall
        } else if (change < -2) {
            templates = NEWS_PATTERNS[3]; // Decline
        } else {
            templates = NEWS_PATTERNS[4]; // Flat
        }
        
        String template = templates[random.nextInt(templates.length)];
        
        return template
            .replace("{company}", companyName)
            .replace("{ticker}", ticker)
            .replace("{percent}", String.format("%.1f", Math.abs(change)))
            .replace("{price}", String.format("%.0f", price));
    }
    
    /**
     * Generate news content (Flask-style)
     */
    private String generateContent(String companyName, String ticker, double change, double price) {
        StringBuilder content = new StringBuilder();
        content.append(String.format("Saham %s (%s) diperdagangkan di Rp%.0f, ", 
            companyName, ticker, price));
        
        if (change > 2) {
            content.append(String.format("mencatat kenaikan %.1f%%. ", Math.abs(change)));
            content.append("Sentimen bullish kuat dari investor institusional. ");
            content.append("Momentum positif diperkirakan berlanjut jangka pendek.");
        } else if (change < -2) {
            content.append(String.format("koreksi %.1f%%. ", Math.abs(change)));
            content.append("Profit taking dan sentimen wait-and-see mendominasi. ");
            content.append("Fundamental perusahaan tetap solid di tengah volatilitas.");
        } else {
            content.append(String.format("bergerak %.1f%%. ", Math.abs(change)));
            content.append("Pasar menunggu katalis baru. ");
            content.append("Akumulasi bertahap dari investor institusional terlihat.");
        }
        
        return content.toString();
    }
    
    /**
     * Get all news (active + old)
     */
    public JSONArray getAllNews() throws JSONException {
        JSONArray allNews = new JSONArray();
        
        for (int i = 0; i < activeNews.length(); i++) {
            allNews.put(activeNews.getJSONObject(i));
        }
        
        for (int i = 0; i < oldNews.length(); i++) {
            allNews.put(oldNews.getJSONObject(i));
        }
        
        return allNews;
    }
}

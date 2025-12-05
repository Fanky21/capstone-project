import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;

/**
 * News Manager
 * Manages news articles
 */
public class NewsManager {
    
    private String filePath;
    private JSONArray activeNews;
    private JSONArray oldNews;
    
    public NewsManager(String filePath) {
        this.filePath = filePath;
        this.activeNews = new JSONArray();
        this.oldNews = new JSONArray();
        loadNews();
    }
    
    /**
     * Load news from JSON file
     */
    private void loadNews() {
        try {
            String content = new String(Files.readAllBytes(Paths.get(filePath)), StandardCharsets.UTF_8);
            JSONArray allNews = new JSONArray(content);
            
            // Split into active (latest 10) and old news
            int count = Math.min(allNews.length(), 10);
            for (int i = 0; i < count; i++) {
                activeNews.put(allNews.get(i));
            }
            
            for (int i = count; i < allNews.length(); i++) {
                oldNews.put(allNews.get(i));
            }
            
            System.out.println("Loaded " + activeNews.length() + " active news and " + oldNews.length() + " old news");
        } catch (IOException | JSONException e) {
            System.err.println("Failed to load news: " + e.getMessage());
        }
    }
    
    /**
     * Get active news (latest 10)
     */
    public JSONArray getActiveNews() {
        return activeNews;
    }
    
    /**
     * Get old news (archived)
     */
    public JSONArray getOldNews() {
        return oldNews;
    }
    
    /**
     * Get specific news by ID
     */
    public JSONObject getNewsById(int newsId) {
        try {
            // Search in active news
            for (int i = 0; i < activeNews.length(); i++) {
                JSONObject article = activeNews.getJSONObject(i);
                if (article.getInt("id") == newsId) {
                    return article;
                }
            }
            
            // Search in old news
            for (int i = 0; i < oldNews.length(); i++) {
                JSONObject article = oldNews.getJSONObject(i);
                if (article.getInt("id") == newsId) {
                    return article;
                }
            }
        } catch (JSONException e) {
            System.err.println("Error getting news by ID: " + e.getMessage());
        }
        return null;
    }
    
    /**
     * Add new news article
     */
    public void addNews(JSONObject article) {
        try {
            // Add to beginning of active news
            JSONArray newActiveNews = new JSONArray();
            newActiveNews.put(article);
            
            for (int i = 0; i < activeNews.length(); i++) {
                newActiveNews.put(activeNews.get(i));
            }
            
            activeNews = newActiveNews;
            
            // If active news exceeds 10, move oldest to old news
            if (activeNews.length() > 10) {
                JSONObject movedArticle = activeNews.getJSONObject(activeNews.length() - 1);
                activeNews.remove(activeNews.length() - 1);
                
                JSONArray newOldNews = new JSONArray();
                newOldNews.put(movedArticle);
                for (int i = 0; i < oldNews.length(); i++) {
                    newOldNews.put(oldNews.get(i));
                }
                oldNews = newOldNews;
            }
        } catch (JSONException e) {
            System.err.println("Error adding news: " + e.getMessage());
        }
    }
}

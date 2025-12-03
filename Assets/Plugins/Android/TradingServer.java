package com.trading.localserver;

import fi.iki.elonen.NanoHTTPD;
import org.json.JSONArray;
import org.json.JSONObject;
import org.json.JSONException;

import java.io.*;
import java.util.*;
import java.text.SimpleDateFormat;
import android.content.Context;

/**
 * Local Trading Server using NanoHTTPD for Android
 * Mimics Flask CoreFLASK system exactly
 */
public class TradingServer extends NanoHTTPD {
    
    private static final int PORT = 5000;
    
    private MoneyManager moneyManager;
    private CompanyDataManager companyDataManager;
    private NewsManager newsManager;
    private Context context;
    
    private Thread priceUpdateThread;
    private Thread newsGenerationThread;
    private volatile boolean isRunning = false;
    
    public TradingServer(Context context, String streamingAssetsPath) throws IOException {
        super(PORT);
        this.context = context;
        
        this.moneyManager = new MoneyManager();
        this.companyDataManager = new CompanyDataManager(context);
        this.newsManager = new NewsManager(context);
        
        android.util.Log.i("TradingServer", "Trading Server initialized");
    }
    
    /**
     * Start the server and background tasks (Flask-style)
     */
    public void startServer() throws IOException {
        start(NanoHTTPD.SOCKET_READ_TIMEOUT, false);
        isRunning = true;
        
        android.util.Log.i("TradingServer", "=== Trading Server started on port " + PORT + " ===");
        
        // Start background threads (like Flask daemon threads)
        startBackgroundTasks();
    }
    
    /**
     * Stop the server
     */
    public void stopServer() {
        isRunning = false;
        stop();
        
        if (priceUpdateThread != null) {
            priceUpdateThread.interrupt();
        }
        if (newsGenerationThread != null) {
            newsGenerationThread.interrupt();
        }
        
        android.util.Log.i("TradingServer", "Trading Server stopped");
    }
    
    /**
     * Main serve method - handles all HTTP requests
     */
    @Override
    public Response serve(IHTTPSession session) {
        String uri = session.getUri();
        Method method = session.getMethod();
        
        android.util.Log.d("TradingServer", method + " " + uri);
        
        try {
            // Root - serve news page
            if (uri.equals("/") || uri.equals("/index") || uri.equals("/news")) {
                return serveTemplate("news.html");
            }
            
            // Markets page
            if (uri.equals("/markets")) {
                return serveTemplate("markets.html");
            }
            
            // Static files
            if (uri.startsWith("/static/")) {
                return serveStaticFile(uri);
            }
            
            // API Routes
            if (uri.startsWith("/api/")) {
                return handleApiRequest(uri, method, session);
            }
            
            // 404 Not Found
            return newFixedLengthResponse(Response.Status.NOT_FOUND, 
                "text/plain", "404 Not Found: " + uri);
                
        } catch (Exception e) {
            android.util.Log.e("TradingServer", "Error serving request", e);
            return newFixedLengthResponse(Response.Status.INTERNAL_ERROR, 
                "text/plain", "500 Internal Server Error: " + e.getMessage());
        }
    }
    
    /**
     * Handle API requests
     */
    private Response handleApiRequest(String uri, Method method, IHTTPSession session) {
        try {
            // Unity Health Check
            if (uri.equals("/api/unity/health")) {
                return createJsonResponse(new JSONObject()
                    .put("status", "ok")
                    .put("message", "Local Trading Server is running")
                    .put("timestamp", System.currentTimeMillis()));
            }
            
            // Unity Money Check
            if (uri.equals("/api/unity/money/check")) {
                return createJsonResponse(new JSONObject()
                    .put("success", true)
                    .put("money", moneyManager.getCash())
                    .put("timestamp", System.currentTimeMillis()));
            }
            
            // Unity Money Operations
            if (uri.startsWith("/api/unity/money/")) {
                return handleMoneyOperation(uri, method, session);
            }
            
            // Get all companies
            if (uri.equals("/api/companies")) {
                JSONArray companies = companyDataManager.getAllCompanies();
                return createJsonResponse(companies);
            }
            
            // Get specific company
            if (uri.startsWith("/api/company/")) {
                String ticker = uri.substring("/api/company/".length());
                JSONObject company = companyDataManager.getCompany(ticker);
                if (company != null) {
                    return createJsonResponse(company);
                } else {
                    return createJsonResponse(new JSONObject()
                        .put("error", "Company not found"), Response.Status.NOT_FOUND);
                }
            }
            
            // Get news
            if (uri.equals("/api/news")) {
                JSONArray news = newsManager.getActiveNews();
                return createJsonResponse(news);
            }
            
            // Get portfolio
            if (uri.equals("/api/portfolio")) {
                JSONObject portfolio = moneyManager.getPortfolioJson(companyDataManager);
                return createJsonResponse(portfolio);
            }
            
            // Get cash
            if (uri.equals("/api/cash")) {
                return createJsonResponse(new JSONObject()
                    .put("cash", moneyManager.getCash()));
            }
            
            // Trade endpoint (unified buy/sell from JavaScript)
            if (uri.equals("/api/trade") && method == Method.POST) {
                return handleTrade(session);
            }
            
            // Buy stock
            if (uri.equals("/api/buy") && method == Method.POST) {
                return handleBuyStock(session);
            }
            
            // Sell stock
            if (uri.equals("/api/sell") && method == Method.POST) {
                return handleSellStock(session);
            }
            
            // Update prices (called from JavaScript)
            if (uri.equals("/api/update-prices") && method == Method.POST) {
                companyDataManager.updateAllPrices();
                return createJsonResponse(new JSONObject()
                    .put("success", true)
                    .put("companies", companyDataManager.getAllCompanies()));
            }
            
            // Reset portfolio
            if (uri.equals("/api/reset") && method == Method.POST) {
                moneyManager.resetPortfolio();
                return createJsonResponse(new JSONObject()
                    .put("message", "Portfolio reset successfully")
                    .put("cash", moneyManager.getCash()));
            }
            
            // Market stats
            if (uri.equals("/api/market-stats")) {
                JSONObject stats = companyDataManager.getMarketStats();
                return createJsonResponse(stats);
            }
            
            return createJsonResponse(new JSONObject()
                .put("error", "API endpoint not found"), Response.Status.NOT_FOUND);
                
        } catch (Exception e) {
            android.util.Log.e("TradingServer", "API Error", e);
            try {
                return createJsonResponse(new JSONObject()
                    .put("error", e.getMessage()), Response.Status.INTERNAL_ERROR);
            } catch (JSONException je) {
                return newFixedLengthResponse(Response.Status.INTERNAL_ERROR, 
                    "text/plain", "Error: " + e.getMessage());
            }
        }
    }
    
    /**
     * Handle money operations
     */
    private Response handleMoneyOperation(String uri, Method method, IHTTPSession session) 
            throws IOException, JSONException, ResponseException {
        
        if (method != Method.POST) {
            return createJsonResponse(new JSONObject()
                .put("success", false)
                .put("error", "Method not allowed"), Response.Status.METHOD_NOT_ALLOWED);
        }
        
        // Parse request body
        Map<String, String> files = new HashMap<>();
        session.parseBody(files);
        
        // Read POST body content
        int contentLength = Integer.parseInt(session.getHeaders().get("content-length"));
        byte[] buffer = new byte[contentLength];
        session.getInputStream().read(buffer, 0, contentLength);
        String postData = new String(buffer);
        
        JSONObject requestJson = new JSONObject(postData);
        double amount = requestJson.getDouble("amount");
        double previousMoney = moneyManager.getCash();
        
        if (uri.equals("/api/unity/money/add")) {
            moneyManager.addCash(amount);
        } else if (uri.equals("/api/unity/money/subtract")) {
            moneyManager.subtractCash(amount);
        } else if (uri.equals("/api/unity/money/set")) {
            moneyManager.setCash(amount);
        } else {
            return createJsonResponse(new JSONObject()
                .put("success", false)
                .put("error", "Unknown money operation"), Response.Status.NOT_FOUND);
        }
        
        return createJsonResponse(new JSONObject()
            .put("success", true)
            .put("message", "Money updated successfully")
            .put("previousMoney", previousMoney)
            .put("currentMoney", moneyManager.getCash())
            .put("timestamp", System.currentTimeMillis()));
    }
    
    /**
     * Handle trade request (unified buy/sell - Flask style)
     */
    private Response handleTrade(IHTTPSession session) throws IOException, JSONException, ResponseException {
        try {
            android.util.Log.i("TradingServer", "=== TRADE REQUEST ===");
            
            // Parse body (NanoHTTPD requires this)
            Map<String, String> files = new HashMap<>();
            session.parseBody(files);
            
            // Get POST data (simple and fast)
            String postData = files.get("postData");
            if (postData == null || postData.isEmpty()) {
                // Fallback: read from input stream
                Map<String, String> params = session.getParms();
                if (params.containsKey("action")) {
                    // Data in params
                    String action = params.get("action");
                    String ticker = params.get("ticker");
                    int shares = Integer.parseInt(params.get("shares"));
                    
                    android.util.Log.i("TradingServer", String.format("Trade: %s %s x%d", action, ticker, shares));
                    return executeTrade(action, ticker, shares);
                }
                
                return createJsonResponse(new JSONObject()
                    .put("error", "No data received"), Response.Status.BAD_REQUEST);
            }
            
            JSONObject requestJson = new JSONObject(postData);
            String action = requestJson.getString("action");
            String ticker = requestJson.getString("ticker");
            int shares = requestJson.getInt("shares");
            
            android.util.Log.i("TradingServer", String.format("Trade: %s %s x%d", action, ticker, shares));
            
            return executeTrade(action, ticker, shares);
            
        } catch (Exception e) {
            android.util.Log.e("TradingServer", "Trade error", e);
            return createJsonResponse(new JSONObject()
                .put("error", e.getMessage()), Response.Status.INTERNAL_ERROR);
        }
    }
    
    /**
     * Execute trade (Flask logic)
     */
    private Response executeTrade(String action, String ticker, int shares) throws JSONException {
        if (shares <= 0) {
            return createJsonResponse(new JSONObject()
                .put("error", "Invalid number of shares"), Response.Status.BAD_REQUEST);
        }
        
        JSONObject company = companyDataManager.getCompany(ticker);
        if (company == null) {
            return createJsonResponse(new JSONObject()
                .put("error", "Company not found"), Response.Status.NOT_FOUND);
        }
        
        double price = company.getDouble("currentPrice");
        
        if ("buy".equals(action)) {
            double cost = price * shares;
            
            if (cost > moneyManager.getCash()) {
                return createJsonResponse(new JSONObject()
                    .put("error", "Insufficient funds"), Response.Status.BAD_REQUEST);
            }
            
            moneyManager.buyStock(ticker, shares, price);
            android.util.Log.i("TradingServer", String.format("BUY SUCCESS: %s x%d @ Rp%.0f", ticker, shares, price));
            
            return createJsonResponse(new JSONObject()
                .put("success", true)
                .put("message", String.format("Successfully bought %d shares of %s", shares, ticker))
                .put("cash", moneyManager.getCash()));
                
        } else if ("sell".equals(action)) {
            int ownedShares = moneyManager.getStockQuantity(ticker);
            
            if (ownedShares < shares) {
                return createJsonResponse(new JSONObject()
                    .put("error", String.format("Insufficient shares (you have %d)", ownedShares)), 
                    Response.Status.BAD_REQUEST);
            }
            
            moneyManager.sellStock(ticker, shares, price);
            android.util.Log.i("TradingServer", String.format("SELL SUCCESS: %s x%d @ Rp%.0f", ticker, shares, price));
            
            return createJsonResponse(new JSONObject()
                .put("success", true)
                .put("message", String.format("Successfully sold %d shares of %s", shares, ticker))
                .put("cash", moneyManager.getCash()));
        }
        
        return createJsonResponse(new JSONObject()
            .put("error", "Invalid action"), Response.Status.BAD_REQUEST);
    }
    
    /**
     * Handle buy stock request
     */
    private Response handleBuyStock(IHTTPSession session) throws IOException, JSONException, ResponseException {
        Map<String, String> files = new HashMap<>();
        session.parseBody(files);
        
        // Read POST body content
        int contentLength = Integer.parseInt(session.getHeaders().get("content-length"));
        byte[] buffer = new byte[contentLength];
        session.getInputStream().read(buffer, 0, contentLength);
        String postData = new String(buffer);
        
        JSONObject requestJson = new JSONObject(postData);
        String ticker = requestJson.getString("ticker");
        int quantity = requestJson.getInt("quantity");
        
        JSONObject company = companyDataManager.getCompany(ticker);
        if (company == null) {
            return createJsonResponse(new JSONObject()
                .put("success", false)
                .put("error", "Company not found"), Response.Status.NOT_FOUND);
        }
        
        double price = company.getDouble("price");
        double totalCost = price * quantity;
        
        if (moneyManager.getCash() < totalCost) {
            return createJsonResponse(new JSONObject()
                .put("success", false)
                .put("error", "Insufficient funds"));
        }
        
        moneyManager.buyStock(ticker, quantity, price);
        
        return createJsonResponse(new JSONObject()
            .put("success", true)
            .put("message", "Stock purchased successfully")
            .put("ticker", ticker)
            .put("quantity", quantity)
            .put("price", price)
            .put("totalCost", totalCost)
            .put("remainingCash", moneyManager.getCash()));
    }
    
    /**
     * Handle sell stock request
     */
    private Response handleSellStock(IHTTPSession session) throws IOException, JSONException, ResponseException {
        Map<String, String> files = new HashMap<>();
        session.parseBody(files);
        
        // Read POST body content
        int contentLength = Integer.parseInt(session.getHeaders().get("content-length"));
        byte[] buffer = new byte[contentLength];
        session.getInputStream().read(buffer, 0, contentLength);
        String postData = new String(buffer);
        
        JSONObject requestJson = new JSONObject(postData);
        String ticker = requestJson.getString("ticker");
        int quantity = requestJson.getInt("quantity");
        
        JSONObject company = companyDataManager.getCompany(ticker);
        if (company == null) {
            return createJsonResponse(new JSONObject()
                .put("success", false)
                .put("error", "Company not found"), Response.Status.NOT_FOUND);
        }
        
        double price = company.getDouble("price");
        
        if (!moneyManager.hasStock(ticker, quantity)) {
            return createJsonResponse(new JSONObject()
                .put("success", false)
                .put("error", "Insufficient stocks"));
        }
        
        moneyManager.sellStock(ticker, quantity, price);
        double totalRevenue = price * quantity;
        
        return createJsonResponse(new JSONObject()
            .put("success", true)
            .put("message", "Stock sold successfully")
            .put("ticker", ticker)
            .put("quantity", quantity)
            .put("price", price)
            .put("totalRevenue", totalRevenue)
            .put("remainingCash", moneyManager.getCash()));
    }
    
    /**
     * Serve HTML template from StreamingAssets
     */
    private Response serveTemplate(String templateName) {
        try {
            InputStream is = context.getAssets().open("server/templates/" + templateName);
            String html = readStream(is);
            
            // Replace Flask url_for() syntax with static paths
            html = html.replaceAll("\\{\\{\\s*url_for\\('static',\\s*filename='([^']+)'\\)\\s*\\}\\}", "/static/$1");
            
            return newFixedLengthResponse(Response.Status.OK, "text/html", html);
        } catch (IOException e) {
            android.util.Log.e("TradingServer", "Template not found: " + templateName, e);
            return newFixedLengthResponse(Response.Status.NOT_FOUND, 
                "text/plain", "Template not found: " + templateName);
        }
    }
    
    /**
     * Serve static file from StreamingAssets
     */
    private Response serveStaticFile(String uri) {
        try {
            // Remove leading /static/
            String filePath = uri.substring("/static/".length());
            InputStream is = context.getAssets().open("server/static/" + filePath);
            
            // Determine MIME type
            String mimeType = "text/plain";
            if (filePath.endsWith(".css")) {
                mimeType = "text/css";
            } else if (filePath.endsWith(".js")) {
                mimeType = "application/javascript";
            } else if (filePath.endsWith(".png")) {
                mimeType = "image/png";
            } else if (filePath.endsWith(".jpg") || filePath.endsWith(".jpeg")) {
                mimeType = "image/jpeg";
            }
            
            return newChunkedResponse(Response.Status.OK, mimeType, is);
        } catch (IOException e) {
            android.util.Log.e("TradingServer", "Static file not found: " + uri, e);
            return newFixedLengthResponse(Response.Status.NOT_FOUND, 
                "text/plain", "File not found: " + uri);
        }
    }
    
    /**
     * Create JSON response
     */
    private Response createJsonResponse(JSONObject json) {
        return createJsonResponse(json, Response.Status.OK);
    }
    
    private Response createJsonResponse(JSONObject json, Response.Status status) {
        return newFixedLengthResponse(status, "application/json", json.toString());
    }
    
    private Response createJsonResponse(JSONArray json) {
        return newFixedLengthResponse(Response.Status.OK, "application/json", json.toString());
    }
    
    /**
     * Read input stream to string
     */
    private String readStream(InputStream is) throws IOException {
        BufferedReader reader = new BufferedReader(new InputStreamReader(is, "UTF-8"));
        StringBuilder sb = new StringBuilder();
        String line;
        while ((line = reader.readLine()) != null) {
            sb.append(line).append("\n");
        }
        reader.close();
        return sb.toString();
    }
    
    /**
     * Start background tasks (Flask-style: daemon threads with sleep loop)
     */
    private void startBackgroundTasks() {
        // Price update thread (like Flask: time.sleep(30))
        priceUpdateThread = new Thread(new Runnable() {
            @Override
            public void run() {
                android.util.Log.i("TradingServer", "Price update thread started");
                
                // Do first update immediately (no sleep first!)
                try {
                    android.util.Log.d("TradingServer", ">>> Initial price update...");
                    companyDataManager.updateAllPrices();
                    android.util.Log.i("TradingServer", ">>> Initial prices updated");
                } catch (Exception e) {
                    android.util.Log.e("TradingServer", "Error in initial price update", e);
                }
                
                while (isRunning) {
                    try {
                        // Sleep AFTER update (like Flask)
                        Thread.sleep(30000); // 30 seconds
                        
                        android.util.Log.d("TradingServer", ">>> Updating prices...");
                        companyDataManager.updateAllPrices();
                        android.util.Log.i("TradingServer", ">>> Prices updated successfully");
                        
                    } catch (InterruptedException e) {
                        android.util.Log.i("TradingServer", "Price update thread interrupted");
                        break;
                    } catch (Exception e) {
                        android.util.Log.e("TradingServer", "Error updating prices", e);
                    }
                }
                
                android.util.Log.i("TradingServer", "Price update thread stopped");
            }
        });
        priceUpdateThread.setDaemon(true);
        priceUpdateThread.start();
        
        // News generation thread (like Flask: time.sleep(30))
        newsGenerationThread = new Thread(new Runnable() {
            @Override
            public void run() {
                android.util.Log.i("TradingServer", "News generation thread started");
                
                // Do first generation immediately
                try {
                    android.util.Log.d("TradingServer", ">>> Initial news generation...");
                    newsManager.generateNews(companyDataManager);
                    android.util.Log.i("TradingServer", ">>> Initial news generated");
                } catch (Exception e) {
                    android.util.Log.e("TradingServer", "Error in initial news generation", e);
                }
                
                while (isRunning) {
                    try {
                        // Sleep AFTER generation (like Flask)
                        Thread.sleep(30000); // 30 seconds
                        
                        android.util.Log.d("TradingServer", ">>> Generating news...");
                        newsManager.generateNews(companyDataManager);
                        android.util.Log.i("TradingServer", ">>> News generated successfully");
                        
                    } catch (InterruptedException e) {
                        android.util.Log.i("TradingServer", "News generation thread interrupted");
                        break;
                    } catch (Exception e) {
                        android.util.Log.e("TradingServer", "Error generating news", e);
                    }
                }
                
                android.util.Log.i("TradingServer", "News generation thread stopped");
            }
        });
        newsGenerationThread.setDaemon(true);
        newsGenerationThread.start();
        
        android.util.Log.i("TradingServer", "=== Background threads started (Flask-style) ===");
    }
}

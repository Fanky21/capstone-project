import com.koushikdutta.async.http.server.AsyncHttpServer;
import com.koushikdutta.async.http.server.AsyncHttpServerRequest;
import com.koushikdutta.async.http.server.AsyncHttpServerResponse;
import com.koushikdutta.async.http.server.HttpServerRequestCallback;

import org.json.JSONArray;
import org.json.JSONObject;
import org.json.JSONException;

import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.text.SimpleDateFormat;
import java.util.*;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

/**
 * Local Trading Server using AndroidAsync
 * Converts Flask application to AndroidAsync HTTP server
 */
public class LocalServer {
    
    private static final int PORT = 5000;
    private static final String BASE_PATH = "Assets/Local Server/";
    
    private AsyncHttpServer server;
    private MoneyManager moneyManager;
    private CompanyDataManager companyDataManager;
    private NewsManager newsManager;
    private ScheduledExecutorService scheduler;
    
    private boolean isRunning = false;
    
    public LocalServer() {
        this.server = new AsyncHttpServer();
        this.moneyManager = new MoneyManager();
        this.companyDataManager = new CompanyDataManager(BASE_PATH + "companies.json");
        this.newsManager = new NewsManager(BASE_PATH + "news.json");
        this.scheduler = Executors.newScheduledThreadPool(1);
    }
    
    /**
     * Start the server
     */
    public void start() {
        if (isRunning) {
            System.out.println("Server is already running!");
            return;
        }
        
        setupRoutes();
        server.listen(PORT);
        isRunning = true;
        
        System.out.println("Local Trading Server started on port " + PORT);
        System.out.println("Access at: http://localhost:" + PORT);
        
        // Start background tasks
        startBackgroundTasks();
    }
    
    /**
     * Stop the server
     */
    public void stop() {
        if (!isRunning) {
            return;
        }
        
        server.stop();
        scheduler.shutdown();
        isRunning = false;
        System.out.println("Local Trading Server stopped");
    }
    
    /**
     * Check if server is running
     */
    public boolean isRunning() {
        return isRunning;
    }
    
    /**
     * Setup all HTTP routes
     */
    private void setupRoutes() {
        
        // ==================== STATIC FILES ====================
        
        // Serve index.html
        server.get("/", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                serveStaticFile(response, BASE_PATH + "index.html", "text/html");
            }
        });
        
        // Serve static files
        server.get("/.*\\.(html|js|css|json)", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                String path = request.getPath().substring(1); // Remove leading /
                String contentType = getContentType(path);
                serveStaticFile(response, BASE_PATH + path, contentType);
            }
        });
        
        // ==================== API ENDPOINTS ====================
        
        // Get all companies
        server.get("/api/companies", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONArray companies = companyDataManager.getAllCompanies();
                    sendJsonResponse(response, 200, companies);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to load companies");
                }
            }
        });
        
        // Get specific company by ticker
        server.get("/api/company/.*", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                String ticker = request.getPath().substring("/api/company/".length());
                
                try {
                    JSONObject company = companyDataManager.getCompany(ticker);
                    if (company != null) {
                        sendJsonResponse(response, 200, company);
                    } else {
                        sendErrorResponse(response, 404, "Company not found");
                    }
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to get company");
                }
            }
        });
        
        // Get all news
        server.get("/api/news", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONArray news = newsManager.getActiveNews();
                    sendJsonResponse(response, 200, news);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to load news");
                }
            }
        });
        
        // Get old news
        server.get("/api/news/old", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONArray news = newsManager.getOldNews();
                    sendJsonResponse(response, 200, news);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to load old news");
                }
            }
        });
        
        // Get portfolio
        server.get("/api/portfolio", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONObject portfolio = moneyManager.getPortfolioWithDetails(companyDataManager);
                    sendJsonResponse(response, 200, portfolio);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to get portfolio");
                }
            }
        });
        
        // Get cash
        server.get("/api/cash", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONObject result = new JSONObject();
                    result.put("cash", moneyManager.getCash());
                    sendJsonResponse(response, 200, result);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to get cash");
                }
            }
        });
        
        // Execute trade (buy/sell)
        server.post("/api/trade", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                request.getBody().get(new AsyncHttpServer.JSONObjectCallback() {
                    @Override
                    public void onCompleted(Exception e, JSONObject body) {
                        if (e != null || body == null) {
                            sendErrorResponse(response, 400, "Invalid request body");
                            return;
                        }
                        
                        try {
                            String action = body.getString("action");
                            String ticker = body.getString("ticker");
                            int shares = body.getInt("shares");
                            
                            if (shares <= 0) {
                                sendErrorResponse(response, 400, "Invalid number of shares");
                                return;
                            }
                            
                            JSONObject company = companyDataManager.getCompany(ticker);
                            if (company == null) {
                                sendErrorResponse(response, 404, "Company not found");
                                return;
                            }
                            
                            double currentPrice = company.getDouble("currentPrice");
                            
                            if ("buy".equals(action)) {
                                handleBuyTrade(response, ticker, shares, currentPrice);
                            } else if ("sell".equals(action)) {
                                handleSellTrade(response, ticker, shares, currentPrice);
                            } else {
                                sendErrorResponse(response, 400, "Invalid action");
                            }
                            
                        } catch (JSONException ex) {
                            sendErrorResponse(response, 400, "Invalid request data");
                        }
                    }
                });
            }
        });
        
        // Update prices
        server.post("/api/update-prices", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    companyDataManager.updatePrices();
                    JSONObject result = new JSONObject();
                    result.put("success", true);
                    result.put("companies", companyDataManager.getAllCompanies());
                    sendJsonResponse(response, 200, result);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to update prices");
                }
            }
        });
        
        // Reset portfolio
        server.post("/api/reset", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    moneyManager.reset();
                    JSONObject result = new JSONObject();
                    result.put("success", true);
                    result.put("message", "Portfolio reset successfully");
                    result.put("cash", moneyManager.getCash());
                    sendJsonResponse(response, 200, result);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to reset portfolio");
                }
            }
        });
        
        // Get market statistics
        server.get("/api/stats", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONObject stats = companyDataManager.getMarketStats();
                    sendJsonResponse(response, 200, stats);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to get stats");
                }
            }
        });
        
        // ==================== UNITY API ENDPOINTS ====================
        
        // Unity: Check money
        server.get("/api/unity/money/check", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONObject result = new JSONObject();
                    result.put("success", true);
                    result.put("money", moneyManager.getCash());
                    result.put("timestamp", getCurrentTimestamp());
                    sendJsonResponse(response, 200, result);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to check money");
                }
            }
        });
        
        // Unity: Add money
        server.post("/api/unity/money/add", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                request.getBody().get(new AsyncHttpServer.JSONObjectCallback() {
                    @Override
                    public void onCompleted(Exception e, JSONObject body) {
                        if (e != null || body == null) {
                            sendErrorResponse(response, 400, "Invalid request body");
                            return;
                        }
                        
                        try {
                            double amount = body.getDouble("amount");
                            
                            if (amount <= 0) {
                                sendErrorResponse(response, 400, "Amount must be positive");
                                return;
                            }
                            
                            double previousMoney = moneyManager.getCash();
                            moneyManager.addCash(amount);
                            
                            JSONObject result = new JSONObject();
                            result.put("success", true);
                            result.put("message", "Added $" + String.format("%.2f", amount));
                            result.put("previousMoney", previousMoney);
                            result.put("currentMoney", moneyManager.getCash());
                            result.put("timestamp", getCurrentTimestamp());
                            
                            sendJsonResponse(response, 200, result);
                        } catch (JSONException ex) {
                            sendErrorResponse(response, 400, "Invalid amount");
                        }
                    }
                });
            }
        });
        
        // Unity: Subtract money
        server.post("/api/unity/money/subtract", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                request.getBody().get(new AsyncHttpServer.JSONObjectCallback() {
                    @Override
                    public void onCompleted(Exception e, JSONObject body) {
                        if (e != null || body == null) {
                            sendErrorResponse(response, 400, "Invalid request body");
                            return;
                        }
                        
                        try {
                            double amount = body.getDouble("amount");
                            
                            if (amount <= 0) {
                                sendErrorResponse(response, 400, "Amount must be positive");
                                return;
                            }
                            
                            if (amount > moneyManager.getCash()) {
                                JSONObject error = new JSONObject();
                                error.put("success", false);
                                error.put("error", "Insufficient funds");
                                error.put("currentMoney", moneyManager.getCash());
                                error.put("requestedAmount", amount);
                                sendJsonResponse(response, 400, error);
                                return;
                            }
                            
                            double previousMoney = moneyManager.getCash();
                            moneyManager.subtractCash(amount);
                            
                            JSONObject result = new JSONObject();
                            result.put("success", true);
                            result.put("message", "Subtracted $" + String.format("%.2f", amount));
                            result.put("previousMoney", previousMoney);
                            result.put("currentMoney", moneyManager.getCash());
                            result.put("timestamp", getCurrentTimestamp());
                            
                            sendJsonResponse(response, 200, result);
                        } catch (JSONException ex) {
                            sendErrorResponse(response, 400, "Invalid amount");
                        }
                    }
                });
            }
        });
        
        // Unity: Set money
        server.post("/api/unity/money/set", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                request.getBody().get(new AsyncHttpServer.JSONObjectCallback() {
                    @Override
                    public void onCompleted(Exception e, JSONObject body) {
                        if (e != null || body == null) {
                            sendErrorResponse(response, 400, "Invalid request body");
                            return;
                        }
                        
                        try {
                            double amount = body.getDouble("amount");
                            
                            if (amount < 0) {
                                sendErrorResponse(response, 400, "Amount cannot be negative");
                                return;
                            }
                            
                            double previousMoney = moneyManager.getCash();
                            moneyManager.setCash(amount);
                            
                            JSONObject result = new JSONObject();
                            result.put("success", true);
                            result.put("message", "Money set to $" + String.format("%.2f", amount));
                            result.put("previousMoney", previousMoney);
                            result.put("currentMoney", moneyManager.getCash());
                            result.put("timestamp", getCurrentTimestamp());
                            
                            sendJsonResponse(response, 200, result);
                        } catch (JSONException ex) {
                            sendErrorResponse(response, 400, "Invalid amount");
                        }
                    }
                });
            }
        });
        
        // Unity: Health check
        server.get("/api/unity/health", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONObject result = new JSONObject();
                    result.put("success", true);
                    result.put("status", "running");
                    result.put("server", "AndroidAsync Trading API");
                    result.put("version", "1.0.0");
                    result.put("timestamp", getCurrentTimestamp());
                    sendJsonResponse(response, 200, result);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Health check failed");
                }
            }
        });
        
        // ==================== MONEY MANAGEMENT API ====================
        
        // Get money
        server.get("/api/money/get", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                try {
                    JSONObject result = new JSONObject();
                    result.put("success", true);
                    result.put("money", moneyManager.getCash());
                    result.put("timestamp", getCurrentTimestamp());
                    sendJsonResponse(response, 200, result);
                } catch (Exception e) {
                    sendErrorResponse(response, 500, "Failed to get money");
                }
            }
        });
        
        // Add money
        server.post("/api/money/add", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                request.getBody().get(new AsyncHttpServer.JSONObjectCallback() {
                    @Override
                    public void onCompleted(Exception e, JSONObject body) {
                        if (e != null || body == null) {
                            sendErrorResponse(response, 400, "No data provided");
                            return;
                        }
                        
                        try {
                            if (!body.has("amount")) {
                                sendErrorResponse(response, 400, "Amount is required");
                                return;
                            }
                            
                            double amount = body.getDouble("amount");
                            
                            if (amount <= 0) {
                                sendErrorResponse(response, 400, "Amount must be positive");
                                return;
                            }
                            
                            double previousMoney = moneyManager.getCash();
                            moneyManager.addCash(amount);
                            
                            JSONObject result = new JSONObject();
                            result.put("success", true);
                            result.put("message", "Successfully added " + amount);
                            result.put("previousMoney", previousMoney);
                            result.put("currentMoney", moneyManager.getCash());
                            result.put("addedAmount", amount);
                            result.put("timestamp", getCurrentTimestamp());
                            
                            sendJsonResponse(response, 200, result);
                        } catch (JSONException ex) {
                            sendErrorResponse(response, 400, "Amount must be a valid number");
                        }
                    }
                });
            }
        });
        
        // Subtract money
        server.post("/api/money/subtract", new HttpServerRequestCallback() {
            @Override
            public void onRequest(AsyncHttpServerRequest request, AsyncHttpServerResponse response) {
                request.getBody().get(new AsyncHttpServer.JSONObjectCallback() {
                    @Override
                    public void onCompleted(Exception e, JSONObject body) {
                        if (e != null || body == null) {
                            sendErrorResponse(response, 400, "No data provided");
                            return;
                        }
                        
                        try {
                            if (!body.has("amount")) {
                                sendErrorResponse(response, 400, "Amount is required");
                                return;
                            }
                            
                            double amount = body.getDouble("amount");
                            
                            if (amount <= 0) {
                                sendErrorResponse(response, 400, "Amount must be positive");
                                return;
                            }
                            
                            double currentMoney = moneyManager.getCash();
                            
                            if (amount > currentMoney) {
                                JSONObject error = new JSONObject();
                                error.put("success", false);
                                error.put("error", "Insufficient funds");
                                error.put("currentMoney", currentMoney);
                                error.put("requestedAmount", amount);
                                sendJsonResponse(response, 400, error);
                                return;
                            }
                            
                            double previousMoney = currentMoney;
                            moneyManager.subtractCash(amount);
                            
                            JSONObject result = new JSONObject();
                            result.put("success", true);
                            result.put("message", "Successfully subtracted " + amount);
                            result.put("previousMoney", previousMoney);
                            result.put("currentMoney", moneyManager.getCash());
                            result.put("subtractedAmount", amount);
                            result.put("timestamp", getCurrentTimestamp());
                            
                            sendJsonResponse(response, 200, result);
                        } catch (JSONException ex) {
                            sendErrorResponse(response, 400, "Amount must be a valid number");
                        }
                    }
                });
            }
        });
    }
    
    /**
     * Handle buy trade
     */
    private void handleBuyTrade(AsyncHttpServerResponse response, String ticker, int shares, double currentPrice) {
        try {
            double cost = shares * currentPrice;
            
            if (cost > moneyManager.getCash()) {
                sendErrorResponse(response, 400, "Insufficient funds");
                return;
            }
            
            moneyManager.subtractCash(cost);
            moneyManager.buyShares(ticker, shares, currentPrice);
            
            JSONObject result = new JSONObject();
            result.put("success", true);
            result.put("message", "Successfully bought " + shares + " shares of " + ticker);
            result.put("cash", moneyManager.getCash());
            
            sendJsonResponse(response, 200, result);
        } catch (Exception e) {
            sendErrorResponse(response, 500, "Failed to execute buy trade");
        }
    }
    
    /**
     * Handle sell trade
     */
    private void handleSellTrade(AsyncHttpServerResponse response, String ticker, int shares, double currentPrice) {
        try {
            if (!moneyManager.hasShares(ticker, shares)) {
                sendErrorResponse(response, 400, "Insufficient shares");
                return;
            }
            
            double revenue = shares * currentPrice;
            moneyManager.addCash(revenue);
            moneyManager.sellShares(ticker, shares);
            
            JSONObject result = new JSONObject();
            result.put("success", true);
            result.put("message", "Successfully sold " + shares + " shares of " + ticker);
            result.put("cash", moneyManager.getCash());
            
            sendJsonResponse(response, 200, result);
        } catch (Exception e) {
            sendErrorResponse(response, 500, "Failed to execute sell trade");
        }
    }
    
    /**
     * Start background tasks (price updates, etc.)
     */
    private void startBackgroundTasks() {
        // Update prices every 30 seconds
        scheduler.scheduleAtFixedRate(new Runnable() {
            @Override
            public void run() {
                try {
                    companyDataManager.updatePrices();
                    System.out.println("Prices updated");
                } catch (Exception e) {
                    System.err.println("Failed to update prices: " + e.getMessage());
                }
            }
        }, 30, 30, TimeUnit.SECONDS);
    }
    
    /**
     * Serve static file
     */
    private void serveStaticFile(AsyncHttpServerResponse response, String filePath, String contentType) {
        try {
            String content = new String(Files.readAllBytes(Paths.get(filePath)), StandardCharsets.UTF_8);
            response.setContentType(contentType);
            response.send(content);
        } catch (IOException e) {
            response.code(404);
            response.send("File not found");
        }
    }
    
    /**
     * Send JSON response
     */
    private void sendJsonResponse(AsyncHttpServerResponse response, int code, Object data) {
        response.code(code);
        response.setContentType("application/json");
        response.send(data.toString());
    }
    
    /**
     * Send error response
     */
    private void sendErrorResponse(AsyncHttpServerResponse response, int code, String message) {
        try {
            JSONObject error = new JSONObject();
            error.put("error", message);
            sendJsonResponse(response, code, error);
        } catch (JSONException e) {
            response.code(code);
            response.send("{\"error\":\"" + message + "\"}");
        }
    }
    
    /**
     * Get content type from file extension
     */
    private String getContentType(String path) {
        if (path.endsWith(".html")) return "text/html";
        if (path.endsWith(".js")) return "application/javascript";
        if (path.endsWith(".css")) return "text/css";
        if (path.endsWith(".json")) return "application/json";
        return "text/plain";
    }
    
    /**
     * Get current timestamp
     */
    private String getCurrentTimestamp() {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
        return sdf.format(new Date());
    }
    
    /**
     * Main entry point
     */
    public static void main(String[] args) {
        LocalServer server = new LocalServer();
        server.start();
        
        // Keep server running
        try {
            Thread.currentThread().join();
        } catch (InterruptedException e) {
            server.stop();
        }
    }
}

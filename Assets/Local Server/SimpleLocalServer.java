import com.sun.net.httpserver.HttpServer;
import com.sun.net.httpserver.HttpHandler;
import com.sun.net.httpserver.HttpExchange;

import org.json.JSONArray;
import org.json.JSONObject;
import org.json.JSONException;

import java.io.*;
import java.net.InetSocketAddress;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.text.SimpleDateFormat;
import java.util.*;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

/**
 * Local Trading Server using Java HttpServer
 * Simplified version without AndroidAsync dependency
 */
public class SimpleLocalServer {
    
    private static final int PORT = 5000;
    private static final String BASE_PATH = "";
    
    private HttpServer server;
    private MoneyManager moneyManager;
    private CompanyDataManager companyDataManager;
    private NewsManager newsManager;
    private ScheduledExecutorService scheduler;
    
    private boolean isRunning = false;
    
    public SimpleLocalServer() {
        this.moneyManager = new MoneyManager();
        this.companyDataManager = new CompanyDataManager(BASE_PATH + "companies.json");
        this.newsManager = new NewsManager(BASE_PATH + "news.json");
        this.scheduler = Executors.newScheduledThreadPool(1);
    }
    
    /**
     * Start the server
     */
    public void start() throws IOException {
        if (isRunning) {
            System.out.println("Server is already running!");
            return;
        }
        
        server = HttpServer.create(new InetSocketAddress(PORT), 0);
        setupRoutes();
        server.setExecutor(null);
        server.start();
        isRunning = true;
        
        System.out.println("Local Trading Server started on port " + PORT);
        System.out.println("Access at: http://localhost:" + PORT);
        
        startBackgroundTasks();
    }
    
    /**
     * Stop the server
     */
    public void stop() {
        if (!isRunning) {
            return;
        }
        
        server.stop(0);
        scheduler.shutdown();
        isRunning = false;
        System.out.println("Local Trading Server stopped");
    }
    
    /**
     * Setup all HTTP routes
     */
    private void setupRoutes() {
        // Root - serve news.html (Flask default route)
        server.createContext("/", createTemplateHandler("news.html"));
        
        // Markets page
        server.createContext("/markets", createTemplateHandler("markets.html"));
        
        // Static CSS files
        server.createContext("/static/css/", createStaticHandler("static/css/", "text/css"));
        
        // Static JS files
        server.createContext("/static/js/", createStaticHandler("static/js/", "application/javascript"));
        
        // API endpoints
        server.createContext("/api/companies", createHandler(this::handleGetCompanies));
        server.createContext("/api/company/", createHandler(this::handleGetCompany));
        server.createContext("/api/news", createHandler(this::handleGetNews));
        server.createContext("/api/portfolio", createHandler(this::handleGetPortfolio));
        server.createContext("/api/cash", createHandler(this::handleGetCash));
        server.createContext("/api/trade", createHandler(this::handleTrade));
        server.createContext("/api/update-prices", createHandler(this::handleUpdatePrices));
        server.createContext("/api/reset", createHandler(this::handleReset));
        server.createContext("/api/stats", createHandler(this::handleGetStats));
        
        // Unity API endpoints
        server.createContext("/api/unity/health", createHandler(this::handleUnityHealth));
        server.createContext("/api/unity/money/check", createHandler(this::handleUnityCheckMoney));
        server.createContext("/api/unity/money/add", createHandler(this::handleUnityAddMoney));
        server.createContext("/api/unity/money/subtract", createHandler(this::handleUnitySubtractMoney));
        server.createContext("/api/unity/money/set", createHandler(this::handleUnitySetMoney));
        
        // Money management API
        server.createContext("/api/money/get", createHandler(this::handleMoneyGet));
        server.createContext("/api/money/add", createHandler(this::handleMoneyAdd));
        server.createContext("/api/money/subtract", createHandler(this::handleMoneySubtract));
    }
    
    /**
     * Create handler wrapper
     */
    private HttpHandler createHandler(RequestHandler handler) {
        return exchange -> {
            try {
                handler.handle(exchange);
            } catch (Exception e) {
                sendErrorResponse(exchange, 500, "Internal server error: " + e.getMessage());
            }
        };
    }
    
    /**
     * Create static file handler
     */
    private HttpHandler createStaticFileHandler(String fileName, String contentType) {
        return exchange -> {
            try {
                if (!"GET".equals(exchange.getRequestMethod())) {
                    exchange.sendResponseHeaders(405, -1);
                    return;
                }
                
                File file = new File(BASE_PATH + fileName);
                if (!file.exists()) {
                    sendErrorResponse(exchange, 404, "File not found");
                    return;
                }
                
                byte[] content = Files.readAllBytes(file.toPath());
                exchange.getResponseHeaders().set("Content-Type", contentType);
                exchange.sendResponseHeaders(200, content.length);
                OutputStream os = exchange.getResponseBody();
                os.write(content);
                os.close();
            } catch (Exception e) {
                sendErrorResponse(exchange, 500, "Error serving file");
            }
        };
    }
    
    /**
     * Create template handler (serves HTML files from templates folder)
     */
    private HttpHandler createTemplateHandler(String templateName) {
        return exchange -> {
            try {
                if (!"GET".equals(exchange.getRequestMethod())) {
                    exchange.sendResponseHeaders(405, -1);
                    return;
                }
                
                File file = new File(BASE_PATH + "templates/" + templateName);
                if (!file.exists()) {
                    sendErrorResponse(exchange, 404, "Template not found");
                    return;
                }
                
                // Read template and replace Flask url_for patterns
                String content = new String(Files.readAllBytes(file.toPath()), StandardCharsets.UTF_8);
                
                // Replace Flask template syntax with static paths
                content = content.replaceAll("\\{\\{\\s*url_for\\('static',\\s*filename='css/style\\.css'\\)\\s*\\}\\}", "/static/css/style.css");
                content = content.replaceAll("\\{\\{\\s*url_for\\('static',\\s*filename='js/app\\.js'\\)\\s*\\}\\}", "/static/js/app.js");
                content = content.replaceAll("\\{\\{\\s*url_for\\('static',\\s*filename='js/news\\.js'\\)\\s*\\}\\}", "/static/js/news.js");
                
                byte[] responseBytes = content.getBytes(StandardCharsets.UTF_8);
                exchange.getResponseHeaders().set("Content-Type", "text/html; charset=UTF-8");
                exchange.sendResponseHeaders(200, responseBytes.length);
                OutputStream os = exchange.getResponseBody();
                os.write(responseBytes);
                os.close();
            } catch (Exception e) {
                sendErrorResponse(exchange, 500, "Error serving template: " + e.getMessage());
            }
        };
    }
    
    /**
     * Create static handler (serves files from static directories)
     */
    private HttpHandler createStaticHandler(String basePath, String contentType) {
        return exchange -> {
            try {
                if (!"GET".equals(exchange.getRequestMethod())) {
                    exchange.sendResponseHeaders(405, -1);
                    return;
                }
                
                String requestPath = exchange.getRequestURI().getPath();
                String fileName = requestPath.substring(requestPath.lastIndexOf('/') + 1);
                
                File file = new File(BASE_PATH + basePath + fileName);
                if (!file.exists()) {
                    sendErrorResponse(exchange, 404, "File not found: " + fileName);
                    return;
                }
                
                byte[] content = Files.readAllBytes(file.toPath());
                exchange.getResponseHeaders().set("Content-Type", contentType);
                exchange.getResponseHeaders().set("Cache-Control", "public, max-age=3600");
                exchange.sendResponseHeaders(200, content.length);
                OutputStream os = exchange.getResponseBody();
                os.write(content);
                os.close();
            } catch (Exception e) {
                sendErrorResponse(exchange, 500, "Error serving static file");
            }
        };
    }
    
    // Handler methods
    private void handleGetCompanies(HttpExchange exchange) throws Exception {
        sendJsonResponse(exchange, 200, companyDataManager.getAllCompanies());
    }
    
    private void handleGetCompany(HttpExchange exchange) throws Exception {
        String path = exchange.getRequestURI().getPath();
        String ticker = path.substring("/api/company/".length());
        
        JSONObject company = companyDataManager.getCompany(ticker);
        if (company != null) {
            sendJsonResponse(exchange, 200, company);
        } else {
            sendErrorResponse(exchange, 404, "Company not found");
        }
    }
    
    private void handleGetNews(HttpExchange exchange) throws Exception {
        sendJsonResponse(exchange, 200, newsManager.getActiveNews());
    }
    
    private void handleGetPortfolio(HttpExchange exchange) throws Exception {
        sendJsonResponse(exchange, 200, moneyManager.getPortfolioWithDetails(companyDataManager));
    }
    
    private void handleGetCash(HttpExchange exchange) throws Exception {
        JSONObject result = new JSONObject();
        result.put("cash", moneyManager.getCash());
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleTrade(HttpExchange exchange) throws Exception {
        if (!"POST".equals(exchange.getRequestMethod())) {
            sendErrorResponse(exchange, 405, "Method not allowed");
            return;
        }
        
        String body = readRequestBody(exchange);
        JSONObject data = new JSONObject(body);
        
        String action = data.getString("action");
        String ticker = data.getString("ticker");
        int shares = data.getInt("shares");
        
        if (shares <= 0) {
            sendErrorResponse(exchange, 400, "Invalid number of shares");
            return;
        }
        
        JSONObject company = companyDataManager.getCompany(ticker);
        if (company == null) {
            sendErrorResponse(exchange, 404, "Company not found");
            return;
        }
        
        double currentPrice = company.getDouble("currentPrice");
        
        if ("buy".equals(action)) {
            double cost = shares * currentPrice;
            if (cost > moneyManager.getCash()) {
                sendErrorResponse(exchange, 400, "Insufficient funds");
                return;
            }
            
            moneyManager.subtractCash(cost);
            moneyManager.buyShares(ticker, shares, currentPrice);
            
            JSONObject result = new JSONObject();
            result.put("success", true);
            result.put("message", "Successfully bought " + shares + " shares of " + ticker);
            result.put("cash", moneyManager.getCash());
            sendJsonResponse(exchange, 200, result);
            
        } else if ("sell".equals(action)) {
            if (!moneyManager.hasShares(ticker, shares)) {
                sendErrorResponse(exchange, 400, "Insufficient shares");
                return;
            }
            
            double revenue = shares * currentPrice;
            moneyManager.addCash(revenue);
            moneyManager.sellShares(ticker, shares);
            
            JSONObject result = new JSONObject();
            result.put("success", true);
            result.put("message", "Successfully sold " + shares + " shares of " + ticker);
            result.put("cash", moneyManager.getCash());
            sendJsonResponse(exchange, 200, result);
            
        } else {
            sendErrorResponse(exchange, 400, "Invalid action");
        }
    }
    
    private void handleUpdatePrices(HttpExchange exchange) throws Exception {
        companyDataManager.updatePrices();
        JSONObject result = new JSONObject();
        result.put("success", true);
        result.put("companies", companyDataManager.getAllCompanies());
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleReset(HttpExchange exchange) throws Exception {
        moneyManager.reset();
        JSONObject result = new JSONObject();
        result.put("success", true);
        result.put("message", "Portfolio reset successfully");
        result.put("cash", moneyManager.getCash());
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleGetStats(HttpExchange exchange) throws Exception {
        sendJsonResponse(exchange, 200, companyDataManager.getMarketStats());
    }
    
    private void handleUnityHealth(HttpExchange exchange) throws Exception {
        JSONObject result = new JSONObject();
        result.put("success", true);
        result.put("status", "running");
        result.put("server", "Java HttpServer Trading API");
        result.put("version", "1.0.0");
        result.put("timestamp", getCurrentTimestamp());
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleUnityCheckMoney(HttpExchange exchange) throws Exception {
        JSONObject result = new JSONObject();
        result.put("success", true);
        result.put("money", moneyManager.getCash());
        result.put("timestamp", getCurrentTimestamp());
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleUnityAddMoney(HttpExchange exchange) throws Exception {
        String body = readRequestBody(exchange);
        JSONObject data = new JSONObject(body);
        double amount = data.getDouble("amount");
        
        if (amount <= 0) {
            sendErrorResponse(exchange, 400, "Amount must be positive");
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
        
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleUnitySubtractMoney(HttpExchange exchange) throws Exception {
        String body = readRequestBody(exchange);
        JSONObject data = new JSONObject(body);
        double amount = data.getDouble("amount");
        
        if (amount <= 0) {
            sendErrorResponse(exchange, 400, "Amount must be positive");
            return;
        }
        
        if (amount > moneyManager.getCash()) {
            JSONObject error = new JSONObject();
            error.put("success", false);
            error.put("error", "Insufficient funds");
            error.put("currentMoney", moneyManager.getCash());
            error.put("requestedAmount", amount);
            sendJsonResponse(exchange, 400, error);
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
        
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleUnitySetMoney(HttpExchange exchange) throws Exception {
        String body = readRequestBody(exchange);
        JSONObject data = new JSONObject(body);
        double amount = data.getDouble("amount");
        
        if (amount < 0) {
            sendErrorResponse(exchange, 400, "Amount cannot be negative");
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
        
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleMoneyGet(HttpExchange exchange) throws Exception {
        JSONObject result = new JSONObject();
        result.put("success", true);
        result.put("money", moneyManager.getCash());
        result.put("timestamp", getCurrentTimestamp());
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleMoneyAdd(HttpExchange exchange) throws Exception {
        String body = readRequestBody(exchange);
        JSONObject data = new JSONObject(body);
        
        if (!data.has("amount")) {
            sendErrorResponse(exchange, 400, "Amount is required");
            return;
        }
        
        double amount = data.getDouble("amount");
        
        if (amount <= 0) {
            sendErrorResponse(exchange, 400, "Amount must be positive");
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
        
        sendJsonResponse(exchange, 200, result);
    }
    
    private void handleMoneySubtract(HttpExchange exchange) throws Exception {
        String body = readRequestBody(exchange);
        JSONObject data = new JSONObject(body);
        
        if (!data.has("amount")) {
            sendErrorResponse(exchange, 400, "Amount is required");
            return;
        }
        
        double amount = data.getDouble("amount");
        
        if (amount <= 0) {
            sendErrorResponse(exchange, 400, "Amount must be positive");
            return;
        }
        
        double currentMoney = moneyManager.getCash();
        
        if (amount > currentMoney) {
            JSONObject error = new JSONObject();
            error.put("success", false);
            error.put("error", "Insufficient funds");
            error.put("currentMoney", currentMoney);
            error.put("requestedAmount", amount);
            sendJsonResponse(exchange, 400, error);
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
        
        sendJsonResponse(exchange, 200, result);
    }
    
    /**
     * Start background tasks
     */
    private void startBackgroundTasks() {
        scheduler.scheduleAtFixedRate(() -> {
            try {
                companyDataManager.updatePrices();
                System.out.println("Prices updated");
            } catch (Exception e) {
                System.err.println("Failed to update prices: " + e.getMessage());
            }
        }, 30, 30, TimeUnit.SECONDS);
    }
    
    /**
     * Read request body
     */
    private String readRequestBody(HttpExchange exchange) throws IOException {
        InputStream is = exchange.getRequestBody();
        BufferedReader reader = new BufferedReader(new InputStreamReader(is, StandardCharsets.UTF_8));
        StringBuilder sb = new StringBuilder();
        String line;
        while ((line = reader.readLine()) != null) {
            sb.append(line);
        }
        return sb.toString();
    }
    
    /**
     * Send JSON response
     */
    private void sendJsonResponse(HttpExchange exchange, int code, Object data) throws IOException {
        byte[] response = data.toString().getBytes(StandardCharsets.UTF_8);
        exchange.getResponseHeaders().set("Content-Type", "application/json");
        exchange.getResponseHeaders().set("Access-Control-Allow-Origin", "*");
        exchange.sendResponseHeaders(code, response.length);
        OutputStream os = exchange.getResponseBody();
        os.write(response);
        os.close();
    }
    
    /**
     * Send error response
     */
    private void sendErrorResponse(HttpExchange exchange, int code, String message) throws IOException {
        try {
            JSONObject error = new JSONObject();
            error.put("error", message);
            sendJsonResponse(exchange, code, error);
        } catch (JSONException e) {
            byte[] response = ("{\"error\":\"" + message + "\"}").getBytes(StandardCharsets.UTF_8);
            exchange.sendResponseHeaders(code, response.length);
            OutputStream os = exchange.getResponseBody();
            os.write(response);
            os.close();
        }
    }
    
    /**
     * Get current timestamp
     */
    private String getCurrentTimestamp() {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
        return sdf.format(new Date());
    }
    
    /**
     * Request handler interface
     */
    @FunctionalInterface
    private interface RequestHandler {
        void handle(HttpExchange exchange) throws Exception;
    }
    
    /**
     * Main entry point
     */
    public static void main(String[] args) {
        try {
            SimpleLocalServer server = new SimpleLocalServer();
            server.start();
            
            // Keep server running
            System.out.println("Press Ctrl+C to stop the server");
            Thread.currentThread().join();
        } catch (Exception e) {
            System.err.println("Error starting server: " + e.getMessage());
            e.printStackTrace();
        }
    }
}

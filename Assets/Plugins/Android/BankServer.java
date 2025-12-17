package com.trading.localserver;

import android.content.Context;
import fi.iki.elonen.NanoHTTPD;
import org.json.JSONObject;
import org.json.JSONArray;
import java.io.InputStream;
import java.io.IOException;
import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.util.Map;
import java.util.HashMap;

/**
 * Bank Server - Port 5001
 * Handles deposits and loans with shared balance from port 5000
 */
public class BankServer extends NanoHTTPD {
    private static final int PORT = 5001;
    private Context context;
    private MoneyManager moneyManager; // Shared with TradingServer
    private BankDataManager bankDataManager;
    private boolean isRunning = false;

    public BankServer(Context context, MoneyManager sharedMoneyManager) {
        super(PORT);
        this.context = context;
        this.moneyManager = sharedMoneyManager;
        this.bankDataManager = new BankDataManager(context, moneyManager);
        
        android.util.Log.i("BankServer", "Bank Server created on port " + PORT);
    }

    public void startServer() throws IOException {
        start(NanoHTTPD.SOCKET_READ_TIMEOUT, false);
        isRunning = true;
        
        android.util.Log.i("BankServer", "=== Bank Server started on port " + PORT + " ===");
    }

    public void stopServer() {
        if (isRunning) {
            stop();
            isRunning = false;
            android.util.Log.i("BankServer", "Bank Server stopped");
        }
    }
    
    public boolean isBankServerRunning() {
        return isRunning;
    }

    @Override
    public Response serve(IHTTPSession session) {
        String uri = session.getUri();
        Method method = session.getMethod();
        
        android.util.Log.i("BankServer", "📥 REQUEST: " + method + " " + uri);
        
        try {
            // Serve static files
            if (uri.equals("/") || uri.equals("/index.html")) {
                android.util.Log.i("BankServer", "Serving index.html from server/bank/index.html");
                return serveFile("server/bank/index.html", "text/html");
            }
            if (uri.equals("/test") || uri.equals("/test.html")) {
                android.util.Log.i("BankServer", "Serving test.html from server/bank/test.html");
                return serveFile("server/bank/test.html", "text/html");
            }
            if (uri.startsWith("/static/")) {
                android.util.Log.i("BankServer", "Serving static file: " + uri);
                return serveStaticFile(uri);
            }
            
            // API endpoints
            if (uri.startsWith("/api/")) {
                return handleApiRequest(uri, method, session);
            }
            
            android.util.Log.w("BankServer", "404 Not Found: " + uri);
            return newFixedLengthResponse(Response.Status.NOT_FOUND, "text/plain", "Not Found: " + uri);
            
        } catch (Exception e) {
            android.util.Log.e("BankServer", "❌ Error serving request: " + uri, e);
            android.util.Log.e("BankServer", "Error message: " + e.getMessage());
            android.util.Log.e("BankServer", "Error class: " + e.getClass().getName());
            
            // Return error as HTML for easier debugging
            String errorHtml = "<html><body><h1>Bank Server Error</h1><pre>" + 
                e.getClass().getName() + ": " + e.getMessage() + 
                "\n\nURI: " + uri + "</pre></body></html>";
            return newFixedLengthResponse(Response.Status.INTERNAL_ERROR, "text/html", errorHtml);
        }
    }

    private Response handleApiRequest(String uri, Method method, IHTTPSession session) throws Exception {
        // Balance endpoint (shared with trading server)
        if (uri.equals("/api/balance")) {
            JSONObject response = new JSONObject();
            response.put("balance", moneyManager.getCash());
            return createJsonResponse(response);
        }
        
        // Deposit endpoints
        if (uri.equals("/api/deposits") && method == Method.GET) {
            JSONArray deposits = bankDataManager.getActiveDeposits();
            return createJsonResponse(deposits);
        }
        
        if (uri.equals("/api/deposits/create") && method == Method.POST) {
            // Parse POST body
            Map<String, String> files = new HashMap<>();
            try {
                session.parseBody(files);
            } catch (Exception e) {
                android.util.Log.e("BankServer", "Failed to parse body", e);
            }
            
            String postData = files.get("postData");
            
            if (postData == null || postData.isEmpty()) {
                // Try to read from input stream
                Map<String, String> parms = session.getParms();
                if (parms.containsKey("amount") && parms.containsKey("days")) {
                    double amount = Double.parseDouble(parms.get("amount"));
                    int days = Integer.parseInt(parms.get("days"));
                    JSONObject result = bankDataManager.createDeposit(amount, days);
                    return createJsonResponse(result);
                } else {
                    android.util.Log.e("BankServer", "No data provided in POST");
                    return createJsonResponse(new JSONObject()
                        .put("success", false)
                        .put("error", "No data provided"), Response.Status.BAD_REQUEST);
                }
            }
            
            JSONObject data = new JSONObject(postData);
            double amount = data.getDouble("amount");
            int days = data.getInt("days");
            
            JSONObject result = bankDataManager.createDeposit(amount, days);
            return createJsonResponse(result);
        }
        
        if (uri.startsWith("/api/deposits/withdraw/") && method == Method.POST) {
            String depositId = uri.substring("/api/deposits/withdraw/".length());
            JSONObject result = bankDataManager.withdrawDeposit(depositId);
            return createJsonResponse(result);
        }
        
        // Loan endpoints
        if (uri.equals("/api/loans") && method == Method.GET) {
            JSONArray loans = bankDataManager.getActiveLoans();
            return createJsonResponse(loans);
        }
        
        if (uri.equals("/api/loans/create") && method == Method.POST) {
            // No body needed for loan creation (fixed amount)
            JSONObject result = bankDataManager.createLoan();
            return createJsonResponse(result);
        }
        
        if (uri.startsWith("/api/loans/repay/") && method == Method.POST) {
            String loanId = uri.substring("/api/loans/repay/".length());
            JSONObject result = bankDataManager.repayLoan(loanId);
            return createJsonResponse(result);
        }
        
        // Stats endpoint
        if (uri.equals("/api/stats")) {
            JSONObject stats = bankDataManager.getStats();
            return createJsonResponse(stats);
        }
        
        return newFixedLengthResponse(Response.Status.NOT_FOUND, "text/plain", "API endpoint not found");
    }

    private Response serveFile(String path, String mimeType) throws IOException {
        android.util.Log.i("BankServer", "📂 Attempting to open: " + path);
        
        try {
            InputStream is = context.getAssets().open(path);
            String content = readStream(is);
            
            android.util.Log.i("BankServer", "✅ File loaded successfully, size: " + content.length() + " chars");
            
            Response response = newFixedLengthResponse(Response.Status.OK, mimeType, content);
            response.addHeader("Cache-Control", "no-cache");
            return response;
            
        } catch (IOException e) {
            android.util.Log.e("BankServer", "❌ Failed to open file: " + path, e);
            
            // List available files in directory for debugging
            try {
                String dir = path.substring(0, path.lastIndexOf('/'));
                android.util.Log.i("BankServer", "📁 Listing files in: " + dir);
                String[] files = context.getAssets().list(dir);
                if (files != null && files.length > 0) {
                    for (String file : files) {
                        android.util.Log.i("BankServer", "  - " + file);
                    }
                } else {
                    android.util.Log.w("BankServer", "Directory is empty or doesn't exist");
                }
            } catch (Exception ex) {
                android.util.Log.e("BankServer", "Failed to list directory", ex);
            }
            
            throw e;
        }
    }

    private Response serveStaticFile(String uri) throws IOException {
        String path = "server/bank" + uri;
        String mimeType = getMimeType(uri);
        return serveFile(path, mimeType);
    }

    private String getMimeType(String uri) {
        if (uri.endsWith(".html")) return "text/html";
        if (uri.endsWith(".css")) return "text/css";
        if (uri.endsWith(".js")) return "application/javascript";
        if (uri.endsWith(".json")) return "application/json";
        if (uri.endsWith(".png")) return "image/png";
        if (uri.endsWith(".jpg") || uri.endsWith(".jpeg")) return "image/jpeg";
        return "text/plain";
    }

    private Response createJsonResponse(JSONObject json) {
        return createJsonResponse(json, Response.Status.OK);
    }

    private Response createJsonResponse(JSONObject json, Response.Status status) {
        Response response = newFixedLengthResponse(status, "application/json", json.toString());
        response.addHeader("Access-Control-Allow-Origin", "*");
        return response;
    }

    private Response createJsonResponse(JSONArray json) {
        Response response = newFixedLengthResponse(Response.Status.OK, "application/json", json.toString());
        response.addHeader("Access-Control-Allow-Origin", "*");
        return response;
    }

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
}

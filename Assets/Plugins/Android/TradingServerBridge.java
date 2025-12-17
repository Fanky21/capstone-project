package com.trading.localserver;

import android.app.Activity;
import android.util.Log;
import com.unity3d.player.UnityPlayer;

/**
 * Unity Bridge for Trading Server and Bank Server
 * Allows Unity C# to start/stop both servers
 */
public class TradingServerBridge {
    
    private static final String TAG = "TradingServerBridge";
    private static TradingServer tradingServer;
    private static BankServer bankServer;
    private static MoneyManager sharedMoneyManager;
    private static Activity activity;
    
    /**
     * Initialize the bridge
     */
    public static void initialize() {
        activity = UnityPlayer.currentActivity;
        Log.i(TAG, "TradingServerBridge initialized");
    }
    
    /**
     * Start both trading server (5000) and bank server (5001) with StreamingAssets path
     */
    public static boolean startServer(String streamingAssetsPath) {
        try {
            if (activity == null) {
                initialize();
            }
            
            if (tradingServer != null && bankServer != null) {
                Log.w(TAG, "Servers already running");
                return true;
            }
            
            // Create shared MoneyManager (so both servers use same balance)
            sharedMoneyManager = new MoneyManager();
            
            // Start Trading Server (Port 5000) with shared MoneyManager
            tradingServer = new TradingServer(activity, streamingAssetsPath, sharedMoneyManager);
            tradingServer.startServer();
            Log.i(TAG, "✅ Trading Server started on port 5000");
            
            // Start Bank Server (Port 5001) with shared MoneyManager
            bankServer = new BankServer(activity, sharedMoneyManager);
            bankServer.startServer();
            Log.i(TAG, "✅ Bank Server started on port 5001");
            
            Log.i(TAG, "Both servers started successfully");
            Log.i(TAG, "StreamingAssets: " + streamingAssetsPath);
            Log.i(TAG, "Trading: http://localhost:5000");
            Log.i(TAG, "Bank: http://localhost:5001");
            return true;
            
        } catch (Exception e) {
            Log.e(TAG, "Failed to start servers", e);
            return false;
        }
    }
    
    /**
     * Stop both servers
     */
    public static void stopServer() {
        try {
            if (tradingServer != null) {
                tradingServer.stopServer();
                tradingServer = null;
                Log.i(TAG, "Trading Server stopped");
            }
            
            if (bankServer != null) {
                bankServer.stopServer();
                bankServer = null;
                Log.i(TAG, "Bank Server stopped");
            }
            
            sharedMoneyManager = null;
            Log.i(TAG, "Both servers stopped");
        } catch (Exception e) {
            Log.e(TAG, "Error stopping servers", e);
        }
    }
    
    /**
     * Check if servers are running
     */
    public static boolean isServerRunning() {
        return tradingServer != null && tradingServer.wasStarted() 
            && bankServer != null && bankServer.isBankServerRunning();
    }
    
    /**
     * Get server status
     */
    public static String getServerStatus() {
        if (tradingServer == null && bankServer == null) {
            return "Not Started";
        }
        
        boolean tradingOk = tradingServer != null && tradingServer.wasStarted();
        boolean bankOk = bankServer != null && bankServer.isBankServerRunning();
        
        if (tradingOk && bankOk) {
            return "Both Running (5000 + 5001)";
        } else if (tradingOk) {
            return "Trading Only (5000)";
        } else if (bankOk) {
            return "Bank Only (5001)";
        } else {
            return "Error";
        }
    }
}


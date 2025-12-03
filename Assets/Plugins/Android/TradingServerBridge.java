package com.trading.localserver;

import android.app.Activity;
import android.util.Log;
import com.unity3d.player.UnityPlayer;

/**
 * Unity Bridge for Trading Server
 * Allows Unity C# to start/stop the server
 */
public class TradingServerBridge {
    
    private static final String TAG = "TradingServerBridge";
    private static TradingServer server;
    private static Activity activity;
    
    /**
     * Initialize the bridge
     */
    public static void initialize() {
        activity = UnityPlayer.currentActivity;
        Log.i(TAG, "TradingServerBridge initialized");
    }
    
    /**
     * Start the trading server with StreamingAssets path
     */
    public static boolean startServer(String streamingAssetsPath) {
        try {
            if (activity == null) {
                initialize();
            }
            
            if (server != null) {
                Log.w(TAG, "Server already running");
                return true;
            }
            
            server = new TradingServer(activity, streamingAssetsPath);
            server.startServer();
            
            Log.i(TAG, "Trading Server started successfully");
            Log.i(TAG, "StreamingAssets: " + streamingAssetsPath);
            return true;
            
        } catch (Exception e) {
            Log.e(TAG, "Failed to start server", e);
            return false;
        }
    }
    
    /**
     * Stop the trading server
     */
    public static void stopServer() {
        try {
            if (server != null) {
                server.stopServer();
                server = null;
                Log.i(TAG, "Trading Server stopped");
            }
        } catch (Exception e) {
            Log.e(TAG, "Error stopping server", e);
        }
    }
    
    /**
     * Check if server is running
     */
    public static boolean isServerRunning() {
        return server != null && server.wasStarted();
    }
    
    /**
     * Get server status
     */
    public static String getServerStatus() {
        if (server == null) {
            return "Not Started";
        } else if (server.wasStarted()) {
            return "Running on port 5000";
        } else {
            return "Error";
        }
    }
}

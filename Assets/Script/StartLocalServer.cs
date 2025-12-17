using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Start and manage the Local Trading Server
/// PC/Editor: Uses Java HttpServer process
/// Android: Uses NanoHTTPD embedded in APK
/// </summary>
public class StartLocalServer : MonoBehaviour
{
    // Singleton instance
    private static StartLocalServer instance;
    
    [Header("Server Configuration")]
    [Tooltip("Port number for the local server")]
    public int serverPort = 5000;
    
    [Tooltip("Automatically start server on awake")]
    public bool autoStart = true;
    
    [Tooltip("Server path relative to project root (PC/Editor only)")]
    public string serverPath = "LocalServer";
    
    [Tooltip("Java executable path (PC/Editor only)")]
    public string javaPath = "java";
    
    [Header("Server Status")]
    [SerializeField] private bool isServerRunning = false;
    [SerializeField] private string serverStatus = "Not Started";
    
    private Process serverProcess; // PC/Editor only
    private AndroidJavaClass androidServerBridge; // Android only
    private string serverUrl;
    
    /// <summary>
    /// Get singleton instance
    /// </summary>
    public static StartLocalServer Instance
    {
        get { return instance; }
    }
    
    void Awake()
    {
        // Implement singleton pattern with DontDestroyOnLoad
        if (instance != null && instance != this)
        {
            // Instance already exists, destroy this duplicate
            UnityEngine.Debug.LogWarning("Duplicate StartLocalServer detected. Destroying...");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        UnityEngine.Debug.Log("StartLocalServer set to DontDestroyOnLoad");
        
        serverUrl = $"http://localhost:{serverPort}";
        
        if (autoStart)
        {
            StartServer();
        }
    }
    
    void OnDestroy()
    {
        StopServer();
    }
    
    void OnApplicationQuit()
    {
        StopServer();
    }
    
    /// <summary>
    /// Start the server (Android or PC/Editor)
    /// </summary>
    public void StartServer()
    {
        if (isServerRunning)
        {
            UnityEngine.Debug.LogWarning("Server is already running!");
            return;
        }
        
        #if UNITY_ANDROID && !UNITY_EDITOR
            StartAndroidServer();
        #else
            StartPCServer();
        #endif
    }
    
    /// <summary>
    /// Start server on Android using NanoHTTPD
    /// </summary>
    private void StartAndroidServer()
    {
        try
        {
            // Get Android Java bridge
            androidServerBridge = new AndroidJavaClass("com.trading.localserver.TradingServerBridge");
            
            // Initialize bridge
            androidServerBridge.CallStatic("initialize");
            
            // Start server with StreamingAssets path
            string streamingAssetsPath = Application.streamingAssetsPath;
            bool success = androidServerBridge.CallStatic<bool>("startServer", streamingAssetsPath);
            
            if (success)
            {
                isServerRunning = true;
                serverStatus = "Starting...";
                UnityEngine.Debug.Log("Android Trading Server starting on port " + serverPort);
                UnityEngine.Debug.Log("StreamingAssets path: " + streamingAssetsPath);
                
                // Check server health
                StartCoroutine(CheckServerHealth());
            }
            else
            {
                serverStatus = "Failed to start";
                UnityEngine.Debug.LogError("Failed to start Android server");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Android server error: {e.Message}");
            serverStatus = $"Error: {e.Message}";
            isServerRunning = false;
        }
    }
    
    /// <summary>
    /// Start server on PC/Editor using Java process
    /// </summary>
    private void StartPCServer()
    {
        if (isServerRunning)
        {
            UnityEngine.Debug.LogWarning("Server is already running!");
            return;
        }
        
        try
        {
            // Get project root directory (parent of Assets)
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string fullServerPath = Path.Combine(projectRoot, serverPath);
            
            if (!Directory.Exists(fullServerPath))
            {
                UnityEngine.Debug.LogError($"Server directory not found: {fullServerPath}");
                serverStatus = "Error: Directory not found";
                return;
            }
            
            // Build classpath with all required libraries
            string classpath = BuildClasspath(fullServerPath);
            
            // Prepare Java process
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = javaPath,
                Arguments = $"-cp \"{classpath}\" SimpleLocalServer",
                WorkingDirectory = fullServerPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            serverProcess = new Process { StartInfo = startInfo };
            
            // Capture output for debugging
            serverProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    UnityEngine.Debug.Log($"[Server] {args.Data}");
                }
            };
            
            serverProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    UnityEngine.Debug.LogWarning($"[Server Error] {args.Data}");
                }
            };
            
            // Start the process
            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();
            
            isServerRunning = true;
            serverStatus = "Starting...";
            
            UnityEngine.Debug.Log($"Local Trading Server starting on port {serverPort}...");
            
            // Check server health
            StartCoroutine(CheckServerHealth());
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Failed to start server: {e.Message}");
            serverStatus = $"Error: {e.Message}";
            isServerRunning = false;
        }
    }
    
    /// <summary>
    /// Stop the server (Android or PC/Editor)
    /// </summary>
    public void StopServer()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            StopAndroidServer();
        #else
            StopPCServer();
        #endif
    }
    
    /// <summary>
    /// Stop Android server
    /// </summary>
    private void StopAndroidServer()
    {
        if (!isServerRunning || androidServerBridge == null)
        {
            return;
        }
        
        try
        {
            androidServerBridge.CallStatic("stopServer");
            androidServerBridge = null;
            
            isServerRunning = false;
            serverStatus = "Stopped";
            
            UnityEngine.Debug.Log("Android Trading Server stopped");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error stopping Android server: {e.Message}");
        }
    }
    
    /// <summary>
    /// Stop PC/Editor server
    /// </summary>
    private void StopPCServer()
    {
        if (!isServerRunning || serverProcess == null)
        {
            return;
        }
        
        try
        {
            if (!serverProcess.HasExited)
            {
                serverProcess.Kill();
                serverProcess.WaitForExit(5000); // Wait up to 5 seconds
            }
            
            serverProcess.Dispose();
            serverProcess = null;
            
            isServerRunning = false;
            serverStatus = "Stopped";
            
            UnityEngine.Debug.Log("Local Trading Server stopped");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error stopping server: {e.Message}");
        }
    }
    
    /// <summary>
    /// Restart the server
    /// </summary>
    public void RestartServer()
    {
        StopServer();
        StartServer();
    }
    
    /// <summary>
    /// Check if server is running and healthy
    /// </summary>
    private IEnumerator CheckServerHealth()
    {
        // Wait a bit for server to start
        yield return new WaitForSeconds(2f);
        
        string healthUrl = $"{serverUrl}/api/unity/health";
        
        using (UnityWebRequest request = UnityWebRequest.Get(healthUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                serverStatus = "Running";
                UnityEngine.Debug.Log($"Server is running at {serverUrl}");
                UnityEngine.Debug.Log($"Server response: {request.downloadHandler.text}");
            }
            else
            {
                serverStatus = "Error: Server not responding";
                UnityEngine.Debug.LogWarning($"Server health check failed: {request.error}");
                
                // Retry after a delay
                yield return new WaitForSeconds(3f);
                StartCoroutine(CheckServerHealth());
            }
        }
    }
    
    /// <summary>
    /// Build classpath for Java with all required libraries
    /// </summary>
    private string BuildClasspath(string serverPath)
    {
        // Start with current directory
        string classpath = ".";
        
        // Add libs directory with full path to JAR files
        string libsPath = Path.Combine(serverPath, "libs");
        if (Directory.Exists(libsPath))
        {
            string[] jarFiles = Directory.GetFiles(libsPath, "*.jar");
            foreach (string jar in jarFiles)
            {
                // Use relative path from working directory
                classpath += ";libs\\" + Path.GetFileName(jar);
            }
        }
        
        UnityEngine.Debug.Log($"Server Path: {serverPath}");
        UnityEngine.Debug.Log($"Classpath: {classpath}");
        
        return classpath;
    }
    
    /// <summary>
    /// Get server URL
    /// </summary>
    public string GetServerUrl()
    {
        return serverUrl;
    }
    
    /// <summary>
    /// Check if server is running
    /// </summary>
    public bool IsServerRunning()
    {
        return isServerRunning;
    }
    
    /// <summary>
    /// Get server status
    /// </summary>
    public string GetServerStatus()
    {
        return serverStatus;
    }
    
    // ==================== Unity API Helper Methods ====================
    
    /// <summary>
    /// Check current money via Unity API
    /// </summary>
    public IEnumerator CheckMoney(Action<bool, double> callback)
    {
        string url = $"{serverUrl}/api/unity/money/check";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                ServerResponse serverResponse = JsonUtility.FromJson<ServerResponse>(response);
                
                if (serverResponse.success)
                {
                    callback?.Invoke(true, serverResponse.money);
                }
                else
                {
                    callback?.Invoke(false, 0);
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"Failed to check money: {request.error}");
                callback?.Invoke(false, 0);
            }
        }
    }
    
    /// <summary>
    /// Add money via Unity API
    /// </summary>
    public IEnumerator AddMoney(double amount, Action<bool, string> callback)
    {
        string url = $"{serverUrl}/api/unity/money/add";
        
        MoneyRequest requestData = new MoneyRequest { amount = amount };
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                callback?.Invoke(response.success, response.message);
            }
            else
            {
                callback?.Invoke(false, request.error);
            }
        }
    }
    
    /// <summary>
    /// Subtract money via Unity API
    /// </summary>
    public IEnumerator SubtractMoney(double amount, Action<bool, string> callback)
    {
        string url = $"{serverUrl}/api/unity/money/subtract";
        
        MoneyRequest requestData = new MoneyRequest { amount = amount };
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                callback?.Invoke(response.success, response.message);
            }
            else
            {
                callback?.Invoke(false, request.error);
            }
        }
    }
    
    /// <summary>
    /// Set money via Unity API
    /// </summary>
    public IEnumerator SetMoney(double amount, Action<bool, string> callback)
    {
        string url = $"{serverUrl}/api/unity/money/set";
        
        MoneyRequest requestData = new MoneyRequest { amount = amount };
        string jsonData = JsonUtility.ToJson(requestData);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                callback?.Invoke(response.success, response.message);
            }
            else
            {
                callback?.Invoke(false, request.error);
            }
        }
    }
    
    // ==================== Data Classes ====================
    
    [Serializable]
    private class MoneyRequest
    {
        public double amount;
    }
    
    [Serializable]
    private class ServerResponse
    {
        public bool success;
        public double money;
        public string message;
        public string error;
        public string timestamp;
        public double previousMoney;
        public double currentMoney;
    }
}

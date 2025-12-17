using UnityEngine;

/// <summary>
/// Contoh script untuk menggunakan CanvasToWebview dari script lain
/// Example script for using CanvasToWebview from other scripts
/// </summary>
public class WebviewExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasToWebview canvasToWebview;
    
    [Header("Example URLs")]
    [SerializeField] private string[] exampleURLs = new string[]
    {
        "https://www.google.com",
        "https://www.youtube.com",
        "https://github.com",
        "https://unity.com"
    };
    
    private int currentURLIndex = 0;

    void Start()
    {
        // Get reference jika belum di-assign
        if (canvasToWebview == null)
        {
            canvasToWebview = FindObjectOfType<CanvasToWebview>();
        }
        
        if (canvasToWebview == null)
        {
            Debug.LogError("CanvasToWebview not found in scene!");
        }
    }

    void Update()
    {
        // Contoh keyboard shortcuts
        ExampleKeyboardControls();
    }

    /// <summary>
    /// Contoh kontrol dengan keyboard
    /// </summary>
    private void ExampleKeyboardControls()
    {
        if (canvasToWebview == null) return;
        
        // Space: Toggle webview
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canvasToWebview.IsWebviewActive())
            {
                CloseWebviewExample();
            }
            else
            {
                OpenWebviewExample();
            }
        }
        
        // N: Next URL (cycle through examples)
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextURLExample();
        }
        
        // Escape: Close webview
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseWebviewExample();
        }
    }

    /// <summary>
    /// Contoh: Buka webview dengan URL default
    /// </summary>
    public void OpenWebviewExample()
    {
        Debug.Log("Example: Opening webview");
        canvasToWebview.StartTransformation();
    }

    /// <summary>
    /// Contoh: Close webview
    /// </summary>
    public void CloseWebviewExample()
    {
        Debug.Log("Example: Closing webview");
        canvasToWebview.CloseWebview();
    }

    /// <summary>
    /// Contoh: Ganti ke URL berikutnya
    /// </summary>
    public void NextURLExample()
    {
        if (exampleURLs.Length == 0) return;
        
        currentURLIndex = (currentURLIndex + 1) % exampleURLs.Length;
        string nextURL = exampleURLs[currentURLIndex];
        
        Debug.Log($"Example: Changing URL to {nextURL}");
        canvasToWebview.SetWebviewURL(nextURL);
        
        // Jika webview sudah aktif, otomatis trigger transformasi lagi
        if (canvasToWebview.IsWebviewActive())
        {
            canvasToWebview.CloseWebview();
            Invoke("OpenWebviewExample", 0.5f);
        }
    }

    /// <summary>
    /// Contoh: Buka URL custom
    /// </summary>
    public void OpenCustomURLExample(string url)
    {
        Debug.Log($"Example: Opening custom URL: {url}");
        canvasToWebview.SetWebviewURL(url);
        canvasToWebview.StartTransformation();
    }

    /// <summary>
    /// Contoh: Buka Google Search
    /// </summary>
    public void SearchGoogleExample(string query)
    {
        string searchURL = $"https://www.google.com/search?q={UnityEngine.Networking.UnityWebRequest.EscapeURL(query)}";
        Debug.Log($"Example: Searching Google for: {query}");
        canvasToWebview.SetWebviewURL(searchURL);
        canvasToWebview.StartTransformation();
    }

    /// <summary>
    /// Contoh: Buka YouTube Search
    /// </summary>
    public void SearchYouTubeExample(string query)
    {
        string searchURL = $"https://www.youtube.com/results?search_query={UnityEngine.Networking.UnityWebRequest.EscapeURL(query)}";
        Debug.Log($"Example: Searching YouTube for: {query}");
        canvasToWebview.SetWebviewURL(searchURL);
        canvasToWebview.StartTransformation();
    }

    /// <summary>
    /// Contoh: Check status webview
    /// </summary>
    public void CheckWebviewStatusExample()
    {
        bool isActive = canvasToWebview.IsWebviewActive();
        Debug.Log($"Example: Webview active status: {isActive}");
    }

    // ========== GUI untuk Testing di Editor ==========
    
    void OnGUI()
    {
        if (canvasToWebview == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("=== Webview Example Controls ===");
        
        // Status
        string status = canvasToWebview.IsWebviewActive() ? "ACTIVE" : "INACTIVE";
        GUILayout.Label($"Status: {status}");
        GUILayout.Space(10);
        
        // Basic Controls
        if (GUILayout.Button("Open Webview (Space)"))
        {
            OpenWebviewExample();
        }
        
        if (GUILayout.Button("Close Webview (Esc)"))
        {
            CloseWebviewExample();
        }
        
        GUILayout.Space(10);
        
        // URL Examples
        GUILayout.Label("Quick URLs:");
        if (GUILayout.Button("Google"))
        {
            OpenCustomURLExample("https://www.google.com");
        }
        
        if (GUILayout.Button("YouTube"))
        {
            OpenCustomURLExample("https://www.youtube.com");
        }
        
        if (GUILayout.Button("GitHub"))
        {
            OpenCustomURLExample("https://github.com");
        }
        
        if (GUILayout.Button("Unity"))
        {
            OpenCustomURLExample("https://unity.com");
        }
        
        GUILayout.Space(10);
        
        // Search Examples
        if (GUILayout.Button("Search: Unity Tutorial"))
        {
            SearchGoogleExample("Unity Tutorial");
        }
        
        if (GUILayout.Button("Search YouTube: Unity 3D"))
        {
            SearchYouTubeExample("Unity 3D");
        }
        
        GUILayout.Space(10);
        
        // Cycle URLs
        if (GUILayout.Button("Next URL (N)"))
        {
            NextURLExample();
        }
        
        GUILayout.EndArea();
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

#if UNITY_ANDROID && !UNITY_EDITOR
using System;
#endif

/// <summary>
/// Script untuk mengubah Canvas Image menjadi Webview Interaktif
/// Converts Canvas Image into Interactive Webview
/// SUPPORT: Gree WebView plugin untuk Android (in-app webview)
/// </summary>
public class CanvasToWebview : MonoBehaviour
{
    [Header("Canvas Image Settings")]
    [Tooltip("RawImage atau Image component yang akan ditransformasi")]
    [SerializeField] private Graphic canvasImage; // Bisa RawImage atau Image
    
    [Tooltip("Texture atau Sprite yang ditampilkan sebelum webview")]
    [SerializeField] private Texture2D displayTexture;
    
    [Header("Webview Settings")]
    [Tooltip("URL webview yang akan dibuka")]
    [SerializeField] private string webviewURL = "https://www.google.com";
    
    [Tooltip("Waktu delay sebelum transformasi (detik)")]
    [SerializeField] private float transformDelay = 3f;
    
    [Tooltip("Aktifkan transformasi otomatis")]
    [SerializeField] private bool autoTransform = true;
    
    [Header("Interactive Elements")]
    [Tooltip("Panel container untuk webview UI")]
    [SerializeField] private GameObject webviewPanel;
    
    [Tooltip("Button untuk trigger transformasi manual")]
    [SerializeField] private Button transformButton;
    
    [Tooltip("Button untuk close webview")]
    [SerializeField] private Button closeButton;
    
    [Tooltip("Input field untuk URL")]
    [SerializeField] private InputField urlInput;
    
    [Tooltip("Loading indicator")]
    [SerializeField] private GameObject loadingIndicator;
    
    [Tooltip("Text untuk menampilkan status")]
    [SerializeField] private Text statusText;
    
    [Header("Animation Settings")]
    [Tooltip("Durasi animasi fade")]
    [SerializeField] private float fadeDuration = 0.8f;
    
    [Tooltip("Gunakan scale animation")]
    [SerializeField] private bool useScaleAnimation = true;

    private bool isTransformed = false;
    private CanvasGroup imageCanvasGroup;
    private RectTransform imageRectTransform;

#if UNITY_ANDROID && !UNITY_EDITOR
    private WebViewObject webViewObject;
#endif


    void Start()
    {
        InitializeComponents();
        SetupInteractiveElements();
        
        if (autoTransform)
        {
            StartCoroutine(AutoTransformCoroutine());
        }
    }

    /// <summary>
    /// Inisialisasi komponen-komponen yang diperlukan
    /// </summary>
    private void InitializeComponents()
    {
        // Setup canvas image
        if (canvasImage != null)
        {
            // Add CanvasGroup untuk fade animation
            imageCanvasGroup = canvasImage.GetComponent<CanvasGroup>();
            if (imageCanvasGroup == null)
            {
                imageCanvasGroup = canvasImage.gameObject.AddComponent<CanvasGroup>();
            }
            
            imageRectTransform = canvasImage.GetComponent<RectTransform>();
            
            // Set texture jika menggunakan RawImage
            if (canvasImage is RawImage && displayTexture != null)
            {
                ((RawImage)canvasImage).texture = displayTexture;
            }
            
            canvasImage.gameObject.SetActive(true);
        }
        
        // Hide webview panel initially
        if (webviewPanel != null)
        {
            webviewPanel.SetActive(false);
        }
        
        // Hide loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
        
        // Set initial URL in input field
        if (urlInput != null)
        {
            urlInput.text = webviewURL;
        }
        
        UpdateStatus("Image mode - Ready");
        Debug.Log("CanvasToWebview initialized");
    }

    /// <summary>
    /// Setup elemen-elemen interaktif (buttons, input, dll)
    /// </summary>
    private void SetupInteractiveElements()
    {
        // Transform button
        if (transformButton != null)
        {
            transformButton.onClick.AddListener(StartTransformation);
        }
        
        // Close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseWebview);
        }
        
        // URL input field
        if (urlInput != null)
        {
            urlInput.onEndEdit.AddListener(OnURLChanged);
        }
        
        // Make canvas image clickable untuk transformasi
        if (canvasImage != null && !autoTransform)
        {
            AddClickableComponent(canvasImage.gameObject);
        }
    }

    /// <summary>
    /// Tambahkan komponen untuk membuat image clickable
    /// </summary>
    private void AddClickableComponent(GameObject obj)
    {
        if (obj.GetComponent<Button>() == null)
        {
            Button btn = obj.AddComponent<Button>();
            btn.onClick.AddListener(StartTransformation);
        }
    }

    /// <summary>
    /// Coroutine untuk auto transformation
    /// </summary>
    private IEnumerator AutoTransformCoroutine()
    {
        UpdateStatus($"Auto transform in {transformDelay}s...");
        yield return new WaitForSeconds(transformDelay);
        StartTransformation();
    }

    /// <summary>
    /// Mulai proses transformasi dari image ke webview
    /// </summary>
    public void StartTransformation()
    {
        if (isTransformed)
        {
            Debug.Log("Already transformed to webview");
            return;
        }
        
        Debug.Log("Starting transformation to webview...");
        UpdateStatus("Transforming to webview...");
        StartCoroutine(TransformationSequence());
    }
    
    /// <summary>
    /// Open panel webview dan mulai webview
    /// Public method untuk dipanggil dari Button onClick event
    /// Akan membuka webview seperti awal mulai
    /// </summary>
    public void OpenPanelWebView()
    {
        Debug.Log("OpenPanelWebView called");
        
        // Jika sedang transformed, close dulu baru open lagi
        if (isTransformed)
        {
            Debug.Log("Webview already active, restarting...");
            StartCoroutine(RestartWebviewSequence());
        }
        else
        {
            // Langsung mulai transformasi
            StartTransformation();
        }
    }
    
    /// <summary>
    /// Restart webview sequence (close then open)
    /// </summary>
    private IEnumerator RestartWebviewSequence()
    {
        // Close webview dulu
        yield return StartCoroutine(CloseWebviewSequence());
        
        // Wait sebentar
        yield return new WaitForSeconds(0.3f);
        
        // Open lagi
        StartTransformation();
    }

    /// <summary>
    /// Sequence animasi transformasi
    /// </summary>
    private IEnumerator TransformationSequence()
    {
        // Step 1: Show loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }
        
        // Step 2: Animate canvas image (fade + scale)
        yield return StartCoroutine(AnimateCanvasImage());
        
        // Step 3: Hide canvas image
        if (canvasImage != null)
        {
            canvasImage.gameObject.SetActive(false);
        }
        
        // Step 4: Show webview panel
        if (webviewPanel != null)
        {
            webviewPanel.SetActive(true);
            yield return StartCoroutine(FadeInWebview());
        }
        
        // Step 5: Load webview URL
        yield return StartCoroutine(LoadWebview());
        
        // Step 6: Hide loading indicator
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
        
        isTransformed = true;
        UpdateStatus("Webview active");
        Debug.Log("Transformation complete!");
    }

    /// <summary>
    /// Animasi untuk canvas image (fade out + optional scale)
    /// </summary>
    private IEnumerator AnimateCanvasImage()
    {
        if (imageCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        Vector3 originalScale = imageRectTransform.localScale;
        Vector3 targetScale = useScaleAnimation ? originalScale * 0.8f : originalScale;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;
            
            // Fade out
            imageCanvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            
            // Scale animation
            if (useScaleAnimation)
            {
                imageRectTransform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            }
            
            yield return null;
        }
        
        imageCanvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Fade in effect untuk webview panel
    /// </summary>
    private IEnumerator FadeInWebview()
    {
        if (webviewPanel == null) yield break;
        
        CanvasGroup panelGroup = webviewPanel.GetComponent<CanvasGroup>();
        if (panelGroup == null)
        {
            panelGroup = webviewPanel.AddComponent<CanvasGroup>();
        }
        
        panelGroup.alpha = 0f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            panelGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / (fadeDuration * 0.5f));
            yield return null;
        }
        
        panelGroup.alpha = 1f;
    }

    /// <summary>
    /// Load webview dengan URL yang ditentukan
    /// </summary>
    private IEnumerator LoadWebview()
    {
        UpdateStatus("Loading webview...");
        
        // Simulasi loading
        yield return new WaitForSeconds(0.5f);
        
        // Get URL dari input field jika ada
        string urlToLoad = webviewURL;
        if (urlInput != null && !string.IsNullOrEmpty(urlInput.text))
        {
            urlToLoad = urlInput.text;
        }
        
        // Pastikan URL memiliki protocol
        if (!urlToLoad.StartsWith("http://") && !urlToLoad.StartsWith("https://"))
        {
            urlToLoad = "https://" + urlToLoad;
        }
        
        Debug.Log($"Opening webview URL: {urlToLoad}");
        
#if UNITY_ANDROID && !UNITY_EDITOR
        // ANDROID: Gunakan Gree WebView untuk in-app webview
        LoadWebviewAndroid(urlToLoad);
#elif UNITY_EDITOR
        // EDITOR: Buka browser eksternal untuk testing
        Application.OpenURL(urlToLoad);
        Debug.Log("Editor mode: Opening in external browser. In Android build, will show in-app webview.");
#else
        // Platform lain: fallback ke browser eksternal
        Application.OpenURL(urlToLoad);
#endif
        
        UpdateStatus($"Webview loaded: {urlToLoad}");
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Load webview di Android menggunakan Gree WebView plugin
    /// Webview akan ditampilkan DI DALAM game pada canvas
    /// </summary>
    private void LoadWebviewAndroid(string url)
    {
        // Jika webview sudah ada, destroy dulu
        if (webViewObject != null)
        {
            DestroyWebviewAndroid();
        }
        
        // Buat WebViewObject baru
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        
        // Calculate margins berdasarkan webview panel position & size
        int marginLeft = 0;
        int marginTop = 0;
        int marginRight = 0;
        int marginBottom = 0;
        
        if (webviewPanel != null)
        {
            RectTransform panelRect = webviewPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Get screen position dari panel
                Canvas canvas = webviewPanel.GetComponentInParent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Vector3[] corners = new Vector3[4];
                    panelRect.GetWorldCorners(corners);
                    
                    // Calculate margins
                    marginLeft = (int)corners[0].x;
                    marginBottom = (int)corners[0].y;
                    marginRight = (int)(Screen.width - corners[2].x);
                    marginTop = (int)(Screen.height - corners[2].y);
                    
                    Debug.Log($"Webview margins - L:{marginLeft} T:{marginTop} R:{marginRight} B:{marginBottom}");
                }
            }
        }
        
        // Initialize WebView dengan callbacks
        webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log($"WebView Callback: {msg}");
            },
            err: (msg) =>
            {
                Debug.LogError($"WebView Error: {msg}");
            },
            started: (msg) =>
            {
                Debug.Log($"WebView Started: {msg}");
            },
            hooked: (msg) =>
            {
                Debug.Log($"WebView Hooked: {msg}");
            },
            ld: (msg) =>
            {
                Debug.Log($"WebView Loaded: {msg}");
                UpdateStatus("Webview fully loaded");
            },
            enableWKWebView: true,
            transparent: false,
            zoom: true,
            ua: ""
        );
        
        // Set margins (posisi webview di dalam screen)
        webViewObject.SetMargins(marginLeft, marginTop, marginRight, marginBottom);
        
        // Set visibility
        webViewObject.SetVisibility(true);
        
        // Load URL
        webViewObject.LoadURL(url);
        
        Debug.Log($"Android WebView initialized and loading: {url}");
    }
    
    /// <summary>
    /// Destroy webview Android
    /// </summary>
    private void DestroyWebviewAndroid()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
            Destroy(webViewObject.gameObject);
            webViewObject = null;
            Debug.Log("Android WebView destroyed");
        }
    }
#endif


    /// <summary>
    /// Close webview dan kembali ke canvas image
    /// Public method untuk dipanggil dari Button onClick event
    /// </summary>
    public void CloseWebview()
    {
        if (!isTransformed)
        {
            Debug.Log("Webview not active");
            return;
        }
        
        Debug.Log("Closing webview...");
        StartCoroutine(CloseWebviewSequence());
    }
    
    /// <summary>
    /// Alternative method untuk close webview panel
    /// Bisa dipanggil langsung dari Button onClick di Inspector
    /// </summary>
    public void CloseWebviewPanel()
    {
        CloseWebview();
    }

    /// <summary>
    /// Sequence untuk close webview
    /// </summary>
    private IEnumerator CloseWebviewSequence()
    {
        UpdateStatus("Closing webview...");
        
#if UNITY_ANDROID && !UNITY_EDITOR
        // Destroy Android webview
        DestroyWebviewAndroid();
#endif
        
        // Fade out webview panel
        if (webviewPanel != null)
        {
            CanvasGroup panelGroup = webviewPanel.GetComponent<CanvasGroup>();
            if (panelGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration * 0.5f)
                {
                    elapsed += Time.deltaTime;
                    panelGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / (fadeDuration * 0.5f));
                    yield return null;
                }
            }
            
            webviewPanel.SetActive(false);
        }
        
        // Show canvas image again
        if (canvasImage != null)
        {
            canvasImage.gameObject.SetActive(true);
            
            // Reset opacity and scale
            if (imageCanvasGroup != null)
            {
                imageCanvasGroup.alpha = 1f;
            }
            if (imageRectTransform != null && useScaleAnimation)
            {
                imageRectTransform.localScale = Vector3.one;
            }
        }
        
        isTransformed = false;
        UpdateStatus("Image mode - Ready");
        Debug.Log("Webview closed");
    }

    /// <summary>
    /// Handler ketika URL diubah
    /// </summary>
    private void OnURLChanged(string newURL)
    {
        webviewURL = newURL;
        Debug.Log($"URL changed to: {newURL}");
        
        // Jika webview sudah aktif, reload dengan URL baru
        if (isTransformed)
        {
            StartCoroutine(LoadWebview());
        }
    }

    /// <summary>
    /// Update status text
    /// </summary>
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[CanvasToWebview] {message}");
    }

    /// <summary>
    /// Set URL webview secara programmatic
    /// </summary>
    public void SetWebviewURL(string url)
    {
        webviewURL = url;
        if (urlInput != null)
        {
            urlInput.text = url;
        }
        Debug.Log($"Webview URL set to: {url}");
    }

    /// <summary>
    /// Get status transformasi
    /// </summary>
    public bool IsWebviewActive()
    {
        return isTransformed;
    }

    void OnDestroy()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Cleanup Android webview
        DestroyWebviewAndroid();
#endif
        
        // Cleanup event listeners
        if (transformButton != null)
        {
            transformButton.onClick.RemoveAllListeners();
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
        }
        
        if (urlInput != null)
        {
            urlInput.onEndEdit.RemoveAllListeners();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class PanelScene : MonoBehaviour
{
    public static PanelScene instance;

    [Header("Panel References")]
    [SerializeField] private GameObject panelContainer;
    [SerializeField] private Button closeButton;

    private string panelShownKey; // Key untuk menyimpan apakah panel sudah ditampilkan di scene ini

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Setup panel untuk scene saat ini
        SetupPanel();
    }

    private void SetupPanel()
    {
        if (panelContainer == null)
        {
            Debug.LogError("PanelScene: Panel container tidak ditemukan!");
            return;
        }

        // Buat key berdasarkan nama scene saat ini
        panelShownKey = $"PanelShown_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}";

        // Check apakah panel sudah ditampilkan di scene ini sebelumnya
        bool hasShownPanel = PlayerPrefs.GetInt(panelShownKey, 0) == 1;

        if (!hasShownPanel)
        {
            // Tampilkan panel untuk pertama kalinya di scene ini
            ShowPanel();
            // Tandai bahwa panel sudah ditampilkan di scene ini
            PlayerPrefs.SetInt(panelShownKey, 1);
            PlayerPrefs.Save();
            Debug.Log($"Panel ditampilkan di scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }
        else
        {
            // Panel sudah pernah ditampilkan, hide panel
            HidePanel();
            Debug.Log($"Panel sudah pernah ditampilkan di scene ini, disembunyikan.");
        }

        // Setup button listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }
    }

    private void ShowPanel()
    {
        panelContainer.SetActive(true);
    }

    private void HidePanel()
    {
        panelContainer.SetActive(false);
    }

    private void OnCloseButtonClicked()
    {
        Debug.Log("Panel ditutup oleh player");
        Destroy(panelContainer);
    }

    /// <summary>
    /// Reset state panel untuk scene tertentu agar panel bisa muncul lagi
    /// Dipanggil dari MainMenu saat memulai game baru
    /// </summary>
    public static void ResetPanelState()
    {
        // Hapus semua panel state dari PlayerPrefs
        // Iterate semua scene yang mungkin dan hapus key-nya
        string[] sceneNames = { 
            "Rumah", "Rumah_Sakit", "Taman", "Bank", 
            "Minimarket", "Toko_Furniture", "Main Menu"
        };

        foreach (string sceneName in sceneNames)
        {
            string panelKey = $"PanelShown_{sceneName}";
            if (PlayerPrefs.HasKey(panelKey))
            {
                PlayerPrefs.DeleteKey(panelKey);
            }
        }

        PlayerPrefs.Save();
        Debug.Log("PanelScene state telah direset untuk semua scene");
    }

    /// <summary>
    /// Reset panel state untuk scene spesifik
    /// </summary>
    public static void ResetPanelStateForScene(string sceneName)
    {
        string panelKey = $"PanelShown_{sceneName}";
        if (PlayerPrefs.HasKey(panelKey))
        {
            PlayerPrefs.DeleteKey(panelKey);
            PlayerPrefs.Save();
            Debug.Log($"Panel state direset untuk scene: {sceneName}");
        }
    }
}

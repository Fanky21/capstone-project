using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject kontrolPanel;
    [SerializeField] private GameObject keluarPanel;

    private int sceneToContinue;

    [Header("Menu State Flags")]
    private bool isMainMenuPanel = false;
    private bool isSettingsOpen = false;
    private bool isKontrolOpen = false;
    private bool isKeluarConfirmOpen = false;

    public bool IsMenuOpen => isMainMenuPanel || isSettingsOpen || isKontrolOpen || isKeluarConfirmOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }
    }

    private void HandleEscape()
    {
        if (isKeluarConfirmOpen)
        {
            CancelKeluar();
        }
        else if (isSettingsOpen)
        {
            CloseSettings();
        }
        else if (isKontrolOpen)
        {
            CloseKontrol();
        }
    }

    public void NewGame()
    {
        // Reset player stats sebelum memulai game baru
        ResetPlayerStats();
        SaveCurrentScene(1);
        SceneManager.LoadScene(1);
    }

    public void ContinueGame()
    {
        sceneToContinue = PlayerPrefs.GetInt("SavedScene", 0);

        if (sceneToContinue != 0)
        {
            SceneManager.LoadScene(sceneToContinue);
        }
        else
        {
            Debug.LogWarning("Tidak ada save game yang ditemukan!");
        }
    }

    private void SaveCurrentScene(int sceneIndex)
    {
        PlayerPrefs.SetInt("SavedScene", sceneIndex);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Reset health dan stamina player ke nilai maksimal
    /// </summary>
    private void ResetPlayerStats()
    {
        // Reset health dan stamina ke 100
        PlayerPrefs.SetFloat("CurrentHealth", 100f);
        PlayerPrefs.SetFloat("CurrentStamina", 100f);
        PlayerPrefs.Save();

        Debug.Log("Player stats telah di-reset: Health = 100, Stamina = 100");
    }

    public void OpenMainMenuPanel()
    {
        isMainMenuPanel = true;
        mainMenuPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        isSettingsOpen = true;
        settingsPanel?.SetActive(true);

        isMainMenuPanel = false;
        mainMenuPanel?.SetActive(false);
    }

    public void OpenKontrol()
    {
        isKontrolOpen = true;
        kontrolPanel?.SetActive(true);

        isMainMenuPanel = false;
        mainMenuPanel?.SetActive(false);
    }

    public void OpenKeluarPanel()
    {
        keluarPanel?.SetActive(true);
        isKeluarConfirmOpen = true;

        isMainMenuPanel = false;
        mainMenuPanel?.SetActive(false);
    }

    public void CancelKeluar()
    {
        keluarPanel?.SetActive(false);
        isKeluarConfirmOpen = false;

        isMainMenuPanel = true;
        mainMenuPanel?.SetActive(true);
    }

    public void ConfirmKeluar()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }

    public void CloseSettings()
    {
        isSettingsOpen = false;
        settingsPanel?.SetActive(false);

        isMainMenuPanel = true;
        mainMenuPanel?.SetActive(true);
    }

    public void CloseKontrol()
    {
        isKontrolOpen = false;
        kontrolPanel?.SetActive(false);

        isMainMenuPanel = true;
        mainMenuPanel?.SetActive(true);
    }

    /// <summary>
    /// Load scene dan simpan sebagai current scene
    /// </summary>
    public void LoadSceneWithSave(int sceneIndex)
    {
        SaveCurrentScene(sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Load scene berdasarkan nama dan simpan
    /// </summary>
    public void LoadSceneWithSaveByName(string sceneName)
    {
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity");
        if (sceneIndex >= 0)
        {
            SaveCurrentScene(sceneIndex);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' tidak ditemukan di Build Settings!");
        }
    }

    /// <summary>
    /// Kembali ke Main Menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}

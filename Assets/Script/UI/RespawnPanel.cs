using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class RespawnPanel : MonoBehaviour
{
    public static RespawnPanel instance;

    [Header("Panel References")]
    [SerializeField] private GameObject panelContainer;
    [SerializeField] private Button buttonYa;
    [SerializeField] private Button buttonTidak;
    [SerializeField] private TextMeshProUGUI costText;

    private int respawnCost = 100000;
    private bool isWaitingForChoice = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Setup button listeners
        if (buttonYa != null)
            buttonYa.onClick.AddListener(OnButtonYaClicked);
        
        if (buttonTidak != null)
            buttonTidak.onClick.AddListener(OnButtonTidakClicked);

        // Hide panel initially
        if (panelContainer != null)
            panelContainer.SetActive(false);
    }

    /// <summary>
    /// Menampilkan panel respawn dan menunggu pilihan player
    /// </summary>
    public void ShowRespawnPanel()
    {
        if (panelContainer == null)
        {
            Debug.LogError("RespawnPanel: Panel container tidak ditemukan!");
            return;
        }

        panelContainer.SetActive(true);
        isWaitingForChoice = true;

        // Update cost text
        if (costText != null)
        {
            costText.text = $"Biaya Respawn: Rp{respawnCost:N0}";
        }

        // Freeze time untuk memberikan kesempatan player memilih
        Time.timeScale = 0f;

        Debug.Log("RespawnPanel ditampilkan. Menunggu pilihan player...");
    }

    private void OnButtonYaClicked()
    {
        if (!isWaitingForChoice) return;

        Debug.Log("Player memilih YA - mencoba respawn dengan pembayaran...");

        // Check apakah player punya uang cukup
        if (Player.instance != null)
        {
            if (Player.instance.uang >= respawnCost)
            {
                // Kurangi uang player
                Player.instance.SubtractUang(respawnCost);
                Debug.Log($"Pembayaran Rp{respawnCost:N0} berhasil! Player respawn.");

                // Hide panel dan continue game
                HideRespawnPanel();
                Time.timeScale = 1f;
                isWaitingForChoice = false;
            }
            else
            {
                // Uang tidak cukup - tampilkan peringatan
                Debug.LogWarning($"Uang tidak cukup! Punya: Rp{Player.instance.uang:N0}, Butuh: Rp{respawnCost:N0}");
                StartCoroutine(ShowInsufficientFundsWarning());
            }
        }
    }

    private void OnButtonTidakClicked()
    {
        if (!isWaitingForChoice) return;

        Debug.Log("Player memilih TIDAK - game over dan kembali ke MainMenu...");

        isWaitingForChoice = false;
        Time.timeScale = 1f;

        // Hide panel
        HideRespawnPanel();

        // Reset semua data player
        ResetAllPlayerData();

        // Load MainMenu scene
        SceneManager.LoadScene("MainMenu");
    }

    private void HideRespawnPanel()
    {
        if (panelContainer != null)
            panelContainer.SetActive(false);
    }

    /// <summary>
    /// Reset semua data player (health, stamina, uang, dll)
    /// </summary>
    private void ResetAllPlayerData()
    {
        // Clear player preferences
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("Semua data player telah direset!");
    }

    private IEnumerator ShowInsufficientFundsWarning()
    {
        // Tampilkan peringatan selama beberapa detik
        TextMeshProUGUI warningText = costText;
        Color originalColor = warningText.color;
        warningText.color = Color.red;
        warningText.text = "Uang tidak cukup!";

        yield return new WaitForSecondsRealtime(2f);

        warningText.color = originalColor;
        warningText.text = $"Biaya Respawn: Rp{respawnCost:N0}";
    }

    public void SetRespawnCost(int newCost)
    {
        respawnCost = newCost;
    }

    public int GetRespawnCost()
    {
        return respawnCost;
    }
}

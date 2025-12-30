using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player instance; // Singleton untuk akses global

    [Header("UI Elements")]
    public Slider healthSlider;
    public Slider staminaSlider;
    public TextMeshProUGUI uangPlayer;

    [Header("References")]
    public InitFlask initFlask;

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;

    public int uang
    {
        get
        {
            if (initFlask != null)
                return initFlask.CurrentMoney;
            return 0;
        }
        set
        {

        }
    } 

    private float currentHealth;
    private float currentStamina;

    void Awake()
    {
        instance = this; // Set singleton
    }

    void Start()
    {
        LoadPlayerData();

        // Get InitFlask reference if not assigned
        if (initFlask == null)
            initFlask = FindFirstObjectByType<InitFlask>();

        // Subscribe to money update event
        if (initFlask != null)
        {
            initFlask.OnMoneyUpdated += OnMoneyChanged;
            // Get initial money from server
            initFlask.getFlaskMoney();
        }

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;

        UpdateUangUI();

        StartCoroutine(DecreaseStatsOverTime());
    }

    void OnDestroy()
    {
        // Unsubscribe from event
        if (initFlask != null)
        {
            initFlask.OnMoneyUpdated -= OnMoneyChanged;
        }
    }

    private void OnMoneyChanged(int newMoney)
    {
        UpdateUangUI();
    }

    private IEnumerator DecreaseStatsOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f); // Tunggu 5 detik

            if (currentHealth > 0)
            {
                currentHealth -= 1f;
                healthSlider.value = currentHealth;
                SavePlayerData(); // Simpan setiap perubahan
            }

            if (currentStamina > 0)
            {
                currentStamina -= 1f;
                staminaSlider.value = currentStamina;
                SavePlayerData(); // Simpan setiap perubahan
            }

            if (currentHealth <= 0)
            {
                Debug.Log("Health habis! Game Over!");
                GameOver();
            }

            if (currentStamina <= 0)
            {
                Debug.Log("Stamina habis! Game Over!");
                GameOver();
            }
        }
    }

    public void AddUang(int amount)
    {
        if (initFlask != null)
            initFlask.addFlaskMoney(amount);
        else
            Debug.LogWarning("InitFlask not found!");
        UpdateUangUI();
    }

    public void SubtractUang(int amount)
    {
        if (initFlask != null)
            initFlask.removeFlaskMoney(amount);
        else
            Debug.LogWarning("InitFlask not found!");
        UpdateUangUI();
    }

    private void UpdateUangUI()
    {
        uangPlayer.text = uang.ToString();
    }

    public void AddHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        healthSlider.value = currentHealth;
        SavePlayerData(); // Simpan setiap perubahan
    }

    public void AddStamina(float amount)
    {
        currentStamina += amount;
        if (currentStamina > maxStamina) currentStamina = maxStamina;
        staminaSlider.value = currentStamina;
        SavePlayerData(); // Simpan setiap perubahan
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetFloat("CurrentHealth", currentHealth);
        PlayerPrefs.SetFloat("CurrentStamina", currentStamina);
        // Money is now saved in InitFlask (Flask backend)
        PlayerPrefs.Save(); // Pastikan disimpan
    }

    private void LoadPlayerData()
    {
        currentHealth = PlayerPrefs.GetFloat("CurrentHealth", maxHealth); // Default ke max jika belum ada
        currentStamina = PlayerPrefs.GetFloat("CurrentStamina", maxStamina); // Default ke max jika belum ada
        // Money is now loaded from InitFlask instead of PlayerPrefs
    }

    private void GameOver()
    {
        Debug.Log("Player Mati! Menampilkan panel respawn...");
        
        // Reset health dan stamina
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        SavePlayerData();
        
        // Load scene Rumah_Sakit terlebih dahulu
        if (SceneController.instance != null)
        {
            SceneController.instance.LoadScene("Rumah_Sakit", "");
        }
        else
        {
            SceneManager.LoadScene("Rumah_Sakit");
        }
        
        // Call show respawn panel setelah frame berikutnya (tunggu scene loaded)
        StartCoroutine(ShowRespawnPanelDelayed());
    }

    private System.Collections.IEnumerator ShowRespawnPanelDelayed()
    {
        yield return new WaitForSeconds(1f);
        
        // Find RespawnPanel dynamically
        GameObject respawnPanelGO = GameObject.Find("RespawnPanelManager");
        if (respawnPanelGO != null)
        {
            var respawnPanelComponent = respawnPanelGO.GetComponent("RespawnPanel");
            if (respawnPanelComponent != null)
            {
                System.Reflection.MethodInfo method = respawnPanelComponent.GetType().GetMethod("ShowRespawnPanel");
                if (method != null)
                {
                    method.Invoke(respawnPanelComponent, null);
                    Debug.Log("RespawnPanel ditampilkan!");
                    yield break;
                }
            }
        }
        
        Debug.LogError("RespawnPanel (RespawnPanelManager) tidak ditemukan di scene!");
    }

    /// <summary>
    /// Reset respawn point setelah player memilih untuk respawn
    /// </summary>
    public void ResetRespawnState()
    {
        // Called setelah player berhasil membayar untuk respawn
        Debug.Log("Player respawn state direset");
    }
}
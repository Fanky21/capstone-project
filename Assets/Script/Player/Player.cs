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
            // This is a read-only property from InitFlask
            // Money updates should be done through InitFlask methods
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

        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;

        UpdateUangUI();

        StartCoroutine(DecreaseStatsOverTime());
    }

    private IEnumerator DecreaseStatsOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f); // Tunggu 5 detik

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
        Debug.Log("Game Over - Kembali ke Main Menu");
        
        // Save current scene before going to main menu
        PlayerPrefs.SetInt("SavedScene", SceneManager.GetActiveScene().buildIndex);
        PlayerPrefs.Save();
        
        // Load main menu (scene index 0)
        SceneManager.LoadScene(0);
    }
}
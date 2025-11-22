using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Player : MonoBehaviour
{
    public static Player instance; // Singleton untuk akses global

    [Header("UI Elements")]
    public Slider healthSlider;
    public Slider staminaSlider;
    public TextMeshProUGUI uangPlayer;

    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float maxStamina = 100f;
    public int uang = 100000; 

    private float currentHealth;
    private float currentStamina;

    void Awake()
    {
        instance = this; // Set singleton
    }

    void Start()
    {
        LoadPlayerData();

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
            yield return new WaitForSeconds(5f); // Tunggu 3 detik

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
                Debug.Log("Health habis! Game Over atau efek lain.");
            }

            if (currentStamina <= 0)
            {
                Debug.Log("Stamina habis! Efek kelelahan.");
            }
        }
    }

    public void AddUang(int amount)
    {
        uang += amount;
        UpdateUangUI();
        SavePlayerData(); // Simpan setiap perubahan
    }

    public void SubtractUang(int amount)
    {
        uang -= amount;
        if (uang < 0) uang = 0; // Pastikan tidak negatif
        UpdateUangUI();
        SavePlayerData(); // Simpan setiap perubahan
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
        PlayerPrefs.SetInt("Uang", uang);
        PlayerPrefs.Save(); // Pastikan disimpan
    }

    private void LoadPlayerData()
    {
        currentHealth = PlayerPrefs.GetFloat("CurrentHealth", maxHealth); // Default ke max jika belum ada
        currentStamina = PlayerPrefs.GetFloat("CurrentStamina", maxStamina); // Default ke max jika belum ada
        uang = PlayerPrefs.GetInt("Uang", 1000); // Default ke 1000 jika belum ada
    }
}
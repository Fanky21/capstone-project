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
    public int uang = 1000; // Jumlah uang awal player (sekarang digunakan oleh MakananManager)

    private float currentHealth;
    private float currentStamina;

    void Awake()
    {
        instance = this; // Set singleton
    }

    void Start()
    {
        // Inisialisasi health dan stamina
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        // Setup slider
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;

        // Update UI uang
        UpdateUangUI();

        // Mulai coroutine untuk mengurangi health dan stamina setiap 3 detik
        StartCoroutine(DecreaseStatsOverTime());
    }

    // Coroutine untuk mengurangi health dan stamina setiap 3 detik
    private IEnumerator DecreaseStatsOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f); // Tunggu 3 detik

            // Kurangi health sebanyak 1, tapi tidak kurang dari 0
            if (currentHealth > 0)
            {
                currentHealth -= 1f;
                healthSlider.value = currentHealth;
            }

            // Kurangi stamina sebanyak 1, tapi tidak kurang dari 0
            if (currentStamina > 0)
            {
                currentStamina -= 1f;
                staminaSlider.value = currentStamina;
            }

            // Jika health atau stamina habis, bisa tambahkan logika game over atau efek lain
            if (currentHealth <= 0)
            {
                Debug.Log("Health habis! Game Over atau efek lain.");
                // Misalnya: StopCoroutine atau panggil method game over
            }

            if (currentStamina <= 0)
            {
                Debug.Log("Stamina habis! Efek kelelahan.");
            }
        }
    }

    // Method untuk menambah atau mengurangi uang
    public void AddUang(int amount)
    {
        uang += amount;
        UpdateUangUI();
    }

    public void SubtractUang(int amount)
    {
        uang -= amount;
        if (uang < 0) uang = 0; // Pastikan tidak negatif
        UpdateUangUI();
    }

    // Update UI untuk uang
    private void UpdateUangUI()
    {
        uangPlayer.text = uang.ToString();
    }

    // Method untuk menambah health (misalnya, dari item)
    public void AddHealth(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        healthSlider.value = currentHealth;
    }

    // Method untuk menambah stamina (misalnya, dari item)
    public void AddStamina(float amount)
    {
        currentStamina += amount;
        if (currentStamina > maxStamina) currentStamina = maxStamina;
        staminaSlider.value = currentStamina;
    }
}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoreStatusBar : MonoBehaviour
{
    // Bar Health
    public GameObject healthBarObject;
    private float barStatusHealth;
    private float maxBarStatusHealth = 100f;
    public Slider healthBar;
    // Bar Hunger
    public GameObject hungerBarObject;
    private float barStatusHunger;
    private float maxBarStatusHunger = 100f;
    public Slider hungerBar;
    // Bar Fatigue
    public GameObject fatigueBarObject;
    private float barStatusFatigue;
    private float maxBarStatusFatigue = 100f;
    public Slider fatigueBar;
    // Time
    public TMP_Text timeText;
    private int timeStatus;
    // Money
    public TMP_Text moneyText;
    private float moneyStatus;

    public MainCoreGame mainCoreGame;

    void Update()
    {
        UpdateUI();
    }
    
    void UpdateUI()
    {
        // Update health bar
        if (healthBar != null)
        {
            healthBar.value = barStatusHealth / maxBarStatusHealth;
        }
        
        // Update hunger bar
        if (hungerBar != null)
        {
            hungerBar.value = mainCoreGame.hunger / maxBarStatusHunger;
        }
        
        // Update fatigue bar
        if (fatigueBar != null)
        {
            fatigueBar.value = mainCoreGame.fatigue / maxBarStatusFatigue;
        }
        
        // Update money text with value from MainCoreGame
        if (moneyText != null)
        {
            moneyText.text = mainCoreGame.uang.ToString("F0");
        }
        
        // Update time text with date from MainCoreGame
        if (timeText != null)
        {
            timeText.text = mainCoreGame.tanggal.ToString("dd/MM/yyyy HH:mm");
        }
    }
}

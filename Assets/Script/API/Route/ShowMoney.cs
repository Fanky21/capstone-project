using TMPro;
using UnityEngine;

public class ShowMoney : MonoBehaviour
{
    public TMP_Text moneyText;
    public InitFlask initFlask;

    void Start()
    {
        // Find InitFlask if not assigned
        if (initFlask == null)
        {
            initFlask = FindFirstObjectByType<InitFlask>();
        }

        if (initFlask != null)
        {
            // Subscribe to money update event
            initFlask.OnMoneyUpdated += UpdateMoneyDisplay;
            
            // Get initial money value
            initFlask.getFlaskMoney();
        }
        else
        {
            Debug.LogWarning("InitFlask not found!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (initFlask != null)
        {
            initFlask.OnMoneyUpdated -= UpdateMoneyDisplay;
        }
    }

    private void UpdateMoneyDisplay(int money)
    {
        if (moneyText != null)
        {
            moneyText.text = "Rp " + money.ToString("N0");
        }
    }
}

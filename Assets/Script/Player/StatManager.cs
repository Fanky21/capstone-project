using UnityEngine;

[RequireComponent(typeof(InitFlask))]
public class StatManager : MonoBehaviour
{

    public TMPro.TMP_Text moneyText;
    public InitFlask flaskAPI;
    private readonly float updateInterval = 2f; // Update every 2 seconds
    private float nextUpdateTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flaskAPI = GetComponent<InitFlask>();
        
        // Subscribe to money update event
        if (flaskAPI != null)
        {
            flaskAPI.OnMoneyUpdated += OnMoneyChanged;
            // Initial fetch
            flaskAPI.getFlaskMoney();
        }
        else
        {
            Debug.LogError("InitFlask component not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Periodic update
        if (Time.time >= nextUpdateTime)
        {
            UpdatePlayerMoney();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    public void UpdatePlayerMoney()
    {
        if (flaskAPI != null)
        {
            // Request updated money from Flask
            flaskAPI.getFlaskMoney();
        }
    }
    
    private void OnMoneyChanged(int newMoney)
    {
        Debug.Log("Money updated from Flask API: " + newMoney);
        if (moneyText != null)
        {
            moneyText.text = newMoney.ToString();
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from event
        if (flaskAPI != null)
        {
            flaskAPI.OnMoneyUpdated -= OnMoneyChanged;
        }
    }
}

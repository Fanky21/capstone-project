using UnityEngine;
using System;

public class InitFlask : MonoBehaviour
{
    private int currentMoney = 0;
    
    // Property to get current money
    public int CurrentMoney => currentMoney;
    
    // Event to notify when money is updated
    public event Action<int> OnMoneyUpdated;
    
    public void getFlaskMoney()
    {
        StartCoroutine(GetFlaskMoneyCoroutine());
    }

    private System.Collections.IEnumerator GetFlaskMoneyCoroutine()
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get("http://localhost:5000/api/unity/money/check"))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Get Money: " + request.downloadHandler.text);
                
                // Parse JSON response
                try
                {
                    MoneyResponse response = JsonUtility.FromJson<MoneyResponse>(request.downloadHandler.text);
                    currentMoney = response.money;
                    Debug.Log("Money updated: " + currentMoney);
                    
                    // Notify listeners
                    OnMoneyUpdated?.Invoke(currentMoney);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse money response: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    public void addFlaskMoney(int amount)
    {
        StartCoroutine(AddFlaskMoneyCoroutine(amount));
    }

    private System.Collections.IEnumerator AddFlaskMoneyCoroutine(int amount)
    {
        string jsonData = JsonUtility.ToJson(new MoneyData { amount = amount });
        using (UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest("http://localhost:5000/api/unity/money/add", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Add Money: " + request.downloadHandler.text);
                // Refresh money to update currentMoney and trigger OnMoneyUpdated
                getFlaskMoney();
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    public void removeFlaskMoney(int amount)
    {
        StartCoroutine(RemoveFlaskMoneyCoroutine(amount));
    }

    private System.Collections.IEnumerator RemoveFlaskMoneyCoroutine(int amount)
    {
        string jsonData = JsonUtility.ToJson(new MoneyData { amount = amount });
        using (UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest("http://localhost:5000/api/unity/money/subtract", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("Remove Money: " + request.downloadHandler.text);
                // Refresh money to update currentMoney and trigger OnMoneyUpdated
                getFlaskMoney();
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    [System.Serializable]
    private class MoneyData
    {
        public int amount;
    }
    
    [System.Serializable]
    private class MoneyResponse
    {
        public bool success;
        public int money;
        public string timestamp;
    }

}

using UnityEngine;

/// <summary>
/// Interface untuk mengelola uang pemain
/// Attach ke GameObject yang sama dengan player atau ke GameObject terpisah
/// </summary>
public class PlayerMoneyManager : MonoBehaviour
{
    public static PlayerMoneyManager Instance;

    [SerializeField] private int playerMoney = 50000;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public int GetMoney()
    {
        return playerMoney;
    }

    public void AddMoney(int amount)
    {
        playerMoney += amount;
        Debug.Log($"Uang bertambah: +Rp{amount:N0}. Total: Rp{playerMoney:N0}");
    }

    public bool RemoveMoney(int amount)
    {
        if (playerMoney >= amount)
        {
            playerMoney -= amount;
            Debug.Log($"Uang berkurang: -Rp{amount:N0}. Total: Rp{playerMoney:N0}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Uang tidak cukup! Dibutuhkan: Rp{amount:N0}, Punya: Rp{playerMoney:N0}");
            return false;
        }
    }

    public bool HasEnoughMoney(int amount)
    {
        return playerMoney >= amount;
    }
}

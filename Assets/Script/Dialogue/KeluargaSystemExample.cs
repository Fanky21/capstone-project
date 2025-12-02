using UnityEngine;

/// <summary>
/// Example Usage of Keluarga Money System
/// Attach this script to test or integrate with existing systems
/// </summary>
public class KeluargaSystemExample : MonoBehaviour
{
    private void Update()
    {
        // Example 1: Display current debt
        if (Input.GetKeyDown(KeyCode.D))
        {
            int debt = KeluargaManager.Instance.GetCurrentDebt();
            int multiplier = KeluargaManager.Instance.GetMultiplier();
            Debug.Log($"Current Debt: Rp{debt:N0} (Multiplier: {multiplier}x)");
        }

        // Example 2: Check player money
        if (Input.GetKeyDown(KeyCode.M))
        {
            int money = Player.instance.uang;
            Debug.Log($"Player Money: Rp{money:N0}");
            
            // Tampilkan dialogue keluarga
            KeluargaManager.Instance.MakeMoneyRequest();
        }

        // Example 3: Add money (cheating/reward)
        if (Input.GetKeyDown(KeyCode.A))
        {
            Player.instance.AddUang(50000);
            Debug.Log($"Uang bertambah 50.000. Total: Rp{Player.instance.uang:N0}");
        }

        // Example 4: Force reset debt
        if (Input.GetKeyDown(KeyCode.R))
        {
            KeluargaManager.Instance.ResetDebt();
            Debug.Log("Debt reset!");
        }

        // Example 5: Force family request
        if (Input.GetKeyDown(KeyCode.F))
        {
            KeluargaManager.Instance.MakeMoneyRequest();
        }
    }

    // Called from UI Button
    public void OnMoneyUIClicked()
    {
        int money = Player.instance.uang;
        // Update UI display here
        Debug.Log($"Update UI: Rp{money:N0}");
    }

    // Called from another system
    public void GiveMoneyReward(int amount)
    {
        Player.instance.AddUang(amount);
    }

    // Check if player can afford something
    public bool CanPlayerAfford(int amount)
    {
        return Player.instance.uang >= amount;
    }
}

/*
KEYBINDS FOR TESTING:
D = Display current debt
M = Check player money & Show dialogue keluarga
A = Add 50000 money
R = Reset debt
F = Force family money request

EXAMPLE INTEGRATION:
1. Call Player.instance.uang to get current balance
2. Call Player.instance.SubtractUang(amount) to spend money
3. Call Player.instance.AddUang(amount) to gain money
4. Call KeluargaManager.Instance.GetCurrentDebt() to check debt
5. Call KeluargaManager.Instance.ResetDebt() to clear debt
6. Call KeluargaManager.Instance.MakeMoneyRequest() to show dialogue
*/

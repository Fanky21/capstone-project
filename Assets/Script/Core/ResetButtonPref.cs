using UnityEngine;

public class ResetButtonPref : MonoBehaviour
{
    public void ResetUang()
    {
        PlayerPrefs.DeleteKey("Uang");
        PlayerPrefs.Save();
        
        if (Player.instance != null)
        {
            Player.instance.uang = 100000; // Default
            Player.instance.AddUang(0); // Update UI tanpa merubah nilai
        }

        Debug.Log("Uang Player berhasil direset!");
    }

    public void ResetHealth()
    {
        PlayerPrefs.DeleteKey("CurrentHealth");
        PlayerPrefs.Save();

        if (Player.instance != null)
        {
            Player.instance.AddHealth(Player.instance.maxHealth); 
        }

        Debug.Log("Health Player berhasil direset!");
    }

    public void ResetStamina()
    {
        PlayerPrefs.DeleteKey("CurrentStamina");
        PlayerPrefs.Save();

        if (Player.instance != null)
        {
            Player.instance.AddStamina(Player.instance.maxStamina);
        }

        Debug.Log("Stamina Player berhasil direset!");
    }
}

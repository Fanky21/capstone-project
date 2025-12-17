using UnityEngine;

public class NewGameScript : MonoBehaviour
{
    public void StartNewGame()
    {
        // Inisialisasi data game baru di sini
        Debug.Log("New Game Started!");

        InitFlask apiScript = FindFirstObjectByType<InitFlask>();
        if (apiScript != null)
        {
            apiScript.addFlaskMoney(100000); // Menambahkan 100.000 uang ke API Flask
        }
        else
        {
            Debug.LogWarning("InitFlask tidak ditemukan di scene!");
        }
    }
}

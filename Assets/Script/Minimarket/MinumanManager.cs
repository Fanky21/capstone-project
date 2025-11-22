using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class MinumanBeli
{
    public string namaMinuman;
    public int hargaMinuman;
    public int healthMinuman;
    public int staminaMinuman;
    public Button buyButton;
}

public class MinumanManager : MonoBehaviour
{
    public TextMeshProUGUI kurangHarga;
    public List<MinumanBeli> minumanList;

    void Start()
    {
        foreach (MinumanBeli minuman in minumanList)
        {
            MinumanBeli temp = minuman;
            temp.buyButton.onClick.AddListener(() => BuyItem(temp));
        }
    }

    public void BuyItem(MinumanBeli minuman)
    {
        // Gunakan uang dari Player.cs
        if (Player.instance.uang >= minuman.hargaMinuman)
        {
            Player.instance.SubtractUang(minuman.hargaMinuman); // Kurangi uang via Player
            kurangHarga.text = "" + minuman.hargaMinuman;

            Item newItem = new Item(
                minuman.namaMinuman,
                1,
                minuman.healthMinuman,
                minuman.staminaMinuman
            );

            Inventory.instance.AddMinuman(newItem); // Panggil AddMinuman

            Debug.Log("Berhasil membeli: " + minuman.namaMinuman);
        }
        else
        {
            Debug.Log("Uang tidak cukup!");
        }
    }
}
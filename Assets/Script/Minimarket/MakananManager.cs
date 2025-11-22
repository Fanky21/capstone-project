using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class MakananBeli
{
    public string namaMakanan;
    public int hargaMakanan;
    public int healthMakanan;
    public int staminaMakanan;
    public Button buyButton;
}

public class MakananManager : MonoBehaviour
{
    public TextMeshProUGUI kurangHarga;
    public List<MakananBeli> makananList;

    void Start()
    {
        foreach (MakananBeli makanan in makananList)
        {
            MakananBeli temp = makanan;
            temp.buyButton.onClick.AddListener(() => BuyItem(temp));
        }
    }

    public void BuyItem(MakananBeli makanan)
    {
        // Gunakan uang dari Player.cs
        if (Player.instance.uang >= makanan.hargaMakanan)
        {
            Player.instance.SubtractUang(makanan.hargaMakanan); // Kurangi uang via Player
            kurangHarga.text = "" + makanan.hargaMakanan;

            Item newItem = new Item(
                makanan.namaMakanan,
                1,
                makanan.healthMakanan,
                makanan.staminaMakanan
            );

            Inventory.instance.AddMakanan(newItem); // Panggil AddMakanan

            Debug.Log("Berhasil membeli: " + makanan.namaMakanan);
        }
        else
        {
            Debug.Log("Uang tidak cukup!");
        }
    }
}
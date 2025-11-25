using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable] 
public class ObatanBeli
{
    public string namaObat;
    public int hargaObat;
    public int healthObat;
    public int staminaObat;
    public Button buyButton;
}

public class ObatManager : MonoBehaviour
{
    public TextMeshProUGUI kurangHarga;
    public List<ObatanBeli> obatList;

    void Start()
    {
        foreach (var obat in obatList)
        {
            ObatanBeli thisObat = obat;
            if (thisObat.buyButton != null)
            {
                thisObat.buyButton.onClick.AddListener(() => BuyItem(thisObat));
            }
            else
            {
                Debug.LogWarning("Buy Button belum di-assign: " + thisObat.namaObat);
            }
        }
    }

    public void BuyItem(ObatanBeli obat)
    {
        if (Player.instance.uang >= obat.hargaObat)
        {
            Player.instance.SubtractUang(obat.hargaObat);

            kurangHarga.text = "-" + obat.hargaObat;

            Item newItem = new Item(
                obat.namaObat,
                1,
                obat.healthObat,
                obat.staminaObat
            );

            Inventory.instance.AddObat(newItem);

            Debug.Log("Berhasil membeli: " + obat.namaObat);
        }
        else
        {
            kurangHarga.text = "X";
            Debug.Log("Uang tidak cukup untuk membeli: " + obat.namaObat);
        }
    }
}

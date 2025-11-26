using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable] 
public class FurnitureBeli
{
    public string namaFurniture;
    public int hargaFurniture;
    public Button buyButton;
    public GameObject alreadyBuyPanel;
}

public class FurnitureManager : MonoBehaviour
{
    public TextMeshProUGUI kurangHarga;
    public List<FurnitureBeli> furnitureList;
    
    // Event untuk notify kapan furniture dibeli
    public delegate void OnFurniturePurchased(string furnitureName);
    public static event OnFurniturePurchased FurniturePurchasedEvent;
    
    private void Start()
    {
        InitializeFurnitureButtons();
        // Pastikan kurangHarga terlihat
        if (kurangHarga != null)
        {
            kurangHarga.gameObject.SetActive(true);
            kurangHarga.text = "";
        }
    }
    
    private void InitializeFurnitureButtons()
    {
        foreach (FurnitureBeli furniture in furnitureList)
        {
            // Cek apakah furniture sudah dibeli
            if (IsFurniturePurchased(furniture.namaFurniture))
            {
                furniture.buyButton.gameObject.SetActive(false);
                furniture.alreadyBuyPanel.gameObject.SetActive(true);
            }
            else
            {
                furniture.buyButton.gameObject.SetActive(true);
                furniture.alreadyBuyPanel.gameObject.SetActive(false);
            }
            
            // Setup button listeners
            furniture.buyButton.onClick.AddListener(() => BuyFurniture(furniture));
        }
    }
    
    public void BuyFurniture(FurnitureBeli furniture)
    {
        // Cek apakah player punya cukup uang
        if (Player.instance.uang < furniture.hargaFurniture)
        {
            // Tampilkan pesan tidak cukup uang - TETAP AKTIF
            int kurangAmount = furniture.hargaFurniture - Player.instance.uang;
            UpdateKurangHargaUI(kurangAmount);
            Debug.Log("Uang tidak cukup! Diperlukan: " + furniture.hargaFurniture + ", Saldo: " + Player.instance.uang);
            return;
        }
        
        // Kurangi uang player
        Player.instance.SubtractUang(furniture.hargaFurniture);
        
        // Simpan bahwa furniture sudah dibeli
        PlayerPrefs.SetInt("Furniture_" + furniture.namaFurniture, 1);
        PlayerPrefs.Save();
        
        // Update UI
        furniture.buyButton.gameObject.SetActive(false);
        furniture.alreadyBuyPanel.gameObject.SetActive(true);
        
        // Tampilkan pesan berhasil dibeli - TETAP AKTIF
        UpdateKurangHargaUI(-furniture.hargaFurniture);
        
        // Trigger event untuk FurnitureHome
        FurniturePurchasedEvent?.Invoke(furniture.namaFurniture);
    }
    
    private void UpdateKurangHargaUI(int jumlah)
    {
        if (kurangHarga == null) return;
        
        if (jumlah < 0)
        {
            // Jika negatif, tampilkan sebagai uang berkurang
            kurangHarga.text = "-" + Mathf.Abs(jumlah);
            kurangHarga.color = Color.black; // Warna hijau untuk uang berkurang
        }
        else if (jumlah > 0)
        {
            // Jika positif, tampilkan sebagai kurang uang
            kurangHarga.text = "X";
            kurangHarga.color = Color.red; // Warna merah untuk peringatan
        }
        else
        {
            kurangHarga.text = "";
        }
    }
    
    public bool IsFurniturePurchased(string furnitureName)
    {
        return PlayerPrefs.GetInt("Furniture_" + furnitureName, 0) == 1;
    }
    
    public void ResetFurniture(string furnitureName)
    {
        // Hapus data dari PlayerPrefs
        PlayerPrefs.DeleteKey("Furniture_" + furnitureName);
        PlayerPrefs.Save();
        
        // Update UI di Manager
        foreach (FurnitureBeli furniture in furnitureList)
        {
            if (furniture.namaFurniture == furnitureName)
            {
                furniture.buyButton.gameObject.SetActive(true);
                furniture.alreadyBuyPanel.gameObject.SetActive(false);
                Debug.Log("Reset furniture: " + furnitureName);
                break;
            }
        }
    }
    
    public void ResetAllFurniture()
    {
        foreach (FurnitureBeli furniture in furnitureList)
        {
            PlayerPrefs.DeleteKey("Furniture_" + furniture.namaFurniture);
            furniture.buyButton.gameObject.SetActive(true);
            furniture.alreadyBuyPanel.gameObject.SetActive(false);
        }
        PlayerPrefs.Save();
        Debug.Log("Reset semua furniture!");
    }
}

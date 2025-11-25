using UnityEngine;
using System.Collections.Generic;

public class FurnitureHome : MonoBehaviour
{
    [System.Serializable]
    public class FurnitureObject
    {
        public string namaFurniture;
        public GameObject furnitureGameObject;
    }
    
    public List<FurnitureObject> furnitureObjects = new List<FurnitureObject>();
    
    private void OnEnable()
    {
        // Subscribe ke event dari FurnitureManager
        FurnitureManager.FurniturePurchasedEvent += OnFurniturePurchased;
    }
    
    private void OnDisable()
    {
        // Unsubscribe dari event
        FurnitureManager.FurniturePurchasedEvent -= OnFurniturePurchased;
    }
    
    private void Start()
    {
        // Initialize furniture yang sudah dibeli saat scene di-load
        InitializeOwnedFurniture();
    }
    
    private void InitializeOwnedFurniture()
    {
        foreach (FurnitureObject furniture in furnitureObjects)
        {
            if (IsFurniturePurchased(furniture.namaFurniture))
            {
                furniture.furnitureGameObject.SetActive(true);
            }
            else
            {
                furniture.furnitureGameObject.SetActive(false);
            }
        }
    }
    
    private void OnFurniturePurchased(string furnitureName)
    {
        // Cari furniture dengan nama yang sama dan aktifkan
        foreach (FurnitureObject furniture in furnitureObjects)
        {
            if (furniture.namaFurniture == furnitureName)
            {
                furniture.furnitureGameObject.SetActive(true);
                break;
            }
        }
    }
    
    public void ResetFurniture(string furnitureName)
    {
        // Deaktifkan furniture ketika di-reset
        foreach (FurnitureObject furniture in furnitureObjects)
        {
            if (furniture.namaFurniture == furnitureName)
            {
                furniture.furnitureGameObject.SetActive(false);
                break;
            }
        }
    }
    
    public void ResetAllFurniture()
    {
        // Deaktifkan semua furniture
        foreach (FurnitureObject furniture in furnitureObjects)
        {
            furniture.furnitureGameObject.SetActive(false);
        }
    }
    
    private bool IsFurniturePurchased(string furnitureName)
    {
        return PlayerPrefs.GetInt("Furniture_" + furnitureName, 0) == 1;
    }
}

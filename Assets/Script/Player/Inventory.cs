using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ItemData
{
    public string name;
    public int count;
    public int healthBonus;
    public int staminaBonus;
}

[System.Serializable]
public class InventoryData
{
    public List<ItemData> makananItems = new List<ItemData>();
    public List<ItemData> minumanItems = new List<ItemData>();
}

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    [Header("UI Slot Makanan")]
    public Button itemMakanan;
    public TextMeshProUGUI itemMakananCountText;
    [Header("UI Slot Minuman")]
    public Button itemMinuman;
    public TextMeshProUGUI itemMinumanCountText;
    private List<Item> makananItems = new List<Item>();
    private List<Item> minumanItems = new List<Item>();
    private Item currentMakanan;
    private Item currentMinuman;
    private bool isConsuming = false; // Flag untuk mencegah klik ganda

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LoadInventoryData();

        itemMakanan.onClick.RemoveAllListeners();
        itemMakanan.onClick.AddListener(() => OnClickSlot(currentMakanan, "makanan"));

        itemMinuman.onClick.RemoveAllListeners();
        itemMinuman.onClick.AddListener(() => OnClickSlot(currentMinuman, "minuman"));

        RefreshUI();
    }

    public void AddMakanan(Item itemToAdd)
    {
        foreach (Item item in makananItems)
        {
            if (item.name == itemToAdd.name)
            {
                item.count += itemToAdd.count;
                currentMakanan = item;
                RefreshUI();
                SaveInventoryData(); // Simpan setiap perubahan
                return;
            }
        }

        makananItems.Add(itemToAdd);
        currentMakanan = itemToAdd;
        RefreshUI();
        SaveInventoryData(); // Simpan setiap perubahan
    }

    public void AddMinuman(Item itemToAdd)
    {
        foreach (Item item in minumanItems)
        {
            if (item.name == itemToAdd.name)
            {
                item.count += itemToAdd.count;
                currentMinuman = item;
                RefreshUI();
                SaveInventoryData(); // Simpan setiap perubahan
                return;
            }
        }

        minumanItems.Add(itemToAdd);
        currentMinuman = itemToAdd;
        RefreshUI();
        SaveInventoryData(); // Simpan setiap perubahan
    }

    private void OnClickSlot(Item currentItem, string jenis)
    {
        if (isConsuming || currentItem == null) return; // Cegah klik ganda

        isConsuming = true; // Set flag
        Debug.Log($"Klik slot {jenis} terjadi!");

        currentItem.count--;

        Player.instance.AddHealth(currentItem.healthBonus);
        Player.instance.AddStamina(currentItem.staminaBonus);

        Debug.Log($"{jenis} {currentItem.name} (+{currentItem.healthBonus} HP, +{currentItem.staminaBonus} Stamina)");

        if (currentItem.count <= 0)
        {
            if (jenis == "makanan")
            {
                makananItems.Remove(currentItem);
                currentMakanan = null;
            }
            else if (jenis == "minuman")
            {
                minumanItems.Remove(currentItem);
                currentMinuman = null;
            }
        }

        RefreshUI();
        SaveInventoryData(); // Simpan setiap perubahan
        isConsuming = false; // Reset flag setelah update UI
    }

    private void RefreshUI()
    { 
        if (currentMakanan == null)
        {
            itemMakanan.gameObject.SetActive(false);
            itemMakananCountText.gameObject.SetActive(false);
        }
        else
        {
            itemMakanan.gameObject.SetActive(true);
            itemMakananCountText.gameObject.SetActive(true);
            itemMakananCountText.text = currentMakanan.count.ToString();
        }

        if (currentMinuman == null)
        {
            itemMinuman.gameObject.SetActive(false);
            itemMinumanCountText.gameObject.SetActive(false);
        }
        else
        {
            itemMinuman.gameObject.SetActive(true);
            itemMinumanCountText.gameObject.SetActive(true);
            itemMinumanCountText.text = currentMinuman.count.ToString();
        }
    }

    private void SaveInventoryData()
    {
        InventoryData data = new InventoryData();

        // Convert makananItems ke ItemData
        foreach (Item item in makananItems)
        {
            data.makananItems.Add(new ItemData
            {
                name = item.name,
                count = item.count,
                healthBonus = item.healthBonus,
                staminaBonus = item.staminaBonus
            });
        }

        foreach (Item item in minumanItems)
        {
            data.minumanItems.Add(new ItemData
            {
                name = item.name,
                count = item.count,
                healthBonus = item.healthBonus,
                staminaBonus = item.staminaBonus
            });
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("InventoryData", json);
        PlayerPrefs.Save(); // Pastikan disimpan
    }

    private void LoadInventoryData()
    {
        string json = PlayerPrefs.GetString("InventoryData", "{}");
        InventoryData data = JsonUtility.FromJson<InventoryData>(json);

        makananItems.Clear();
        foreach (ItemData itemData in data.makananItems)
        {
            Item item = new Item(itemData.name, itemData.count, itemData.healthBonus, itemData.staminaBonus);
            makananItems.Add(item);
        }
        if (makananItems.Count > 0) currentMakanan = makananItems[0]; // Set currentMakanan ke item pertama jika ada

        minumanItems.Clear();
        foreach (ItemData itemData in data.minumanItems)
        {
            Item item = new Item(itemData.name, itemData.count, itemData.healthBonus, itemData.staminaBonus);
            minumanItems.Add(item);
        }
        if (minumanItems.Count > 0) currentMinuman = minumanItems[0]; // Set currentMinuman ke item pertama jika ada
    }
}
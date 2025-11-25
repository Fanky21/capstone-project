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
    public List<ItemData> obatItems = new List<ItemData>();
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

    [Header("UI Slot Obat")]
    public Button itemObat;
    public TextMeshProUGUI itemObatCountText;

    private List<Item> makananItems = new List<Item>();
    private List<Item> minumanItems = new List<Item>();
    private List<Item> obatItems = new List<Item>();

    private Item currentMakanan;
    private Item currentMinuman;
    private Item currentObat;

    private bool isConsuming = false; // Flag untuk mencegah klik ganda

    void Awake()
    {
        // Simple singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadInventoryData();

        // Setup listeners (safeguard null checks)
        if (itemMakanan != null)
        {
            itemMakanan.onClick.RemoveAllListeners();
            itemMakanan.onClick.AddListener(() => OnClickSlot(currentMakanan, "makanan"));
        }
        if (itemMinuman != null)
        {
            itemMinuman.onClick.RemoveAllListeners();
            itemMinuman.onClick.AddListener(() => OnClickSlot(currentMinuman, "minuman"));
        }
        if (itemObat != null)
        {
            itemObat.onClick.RemoveAllListeners();
            itemObat.onClick.AddListener(() => OnClickSlot(currentObat, "obat"));
        }

        RefreshUI();
    }

    #region Add Methods
    public void AddMakanan(Item itemToAdd)
    {
        if (itemToAdd == null) return;

        foreach (Item item in makananItems)
        {
            if (item.name == itemToAdd.name)
            {
                item.count += itemToAdd.count;
                currentMakanan = item;
                RefreshUI();
                SaveInventoryData();
                return;
            }
        }

        makananItems.Add(itemToAdd);
        currentMakanan = itemToAdd;
        RefreshUI();
        SaveInventoryData();
    }

    public void AddMinuman(Item itemToAdd)
    {
        if (itemToAdd == null) return;

        foreach (Item item in minumanItems)
        {
            if (item.name == itemToAdd.name)
            {
                item.count += itemToAdd.count;
                currentMinuman = item;
                RefreshUI();
                SaveInventoryData();
                return;
            }
        }

        minumanItems.Add(itemToAdd);
        currentMinuman = itemToAdd;
        RefreshUI();
        SaveInventoryData();
    }

    // NEW: AddObat
    public void AddObat(Item itemToAdd)
    {
        if (itemToAdd == null) return;

        foreach (Item item in obatItems)
        {
            if (item.name == itemToAdd.name)
            {
                item.count += itemToAdd.count;
                currentObat = item;
                RefreshUI();
                SaveInventoryData();
                return;
            }
        }

        obatItems.Add(itemToAdd);
        currentObat = itemToAdd;
        RefreshUI();
        SaveInventoryData();
    }
    #endregion

    private void OnClickSlot(Item currentItem, string jenis)
    {
        if (isConsuming || currentItem == null) return; // Cegah klik ganda

        // Extra safety: cek Player.instance
        if (Player.instance == null)
        {
            Debug.LogWarning("Player.instance is null. Cannot consume item.");
            return;
        }

        isConsuming = true; // Set flag
        Debug.Log($"Klik slot {jenis} terjadi!");

        // Kurangi jumlah
        currentItem.count--;

        // Terapkan efek
        Player.instance.AddHealth(currentItem.healthBonus);
        Player.instance.AddStamina(currentItem.staminaBonus);

        Debug.Log($"{jenis} {currentItem.name} (+{currentItem.healthBonus} HP, +{currentItem.staminaBonus} Stamina)");

        // Jika habis, hapus dari daftar dan reset current
        if (currentItem.count <= 0)
        {
            if (jenis == "makanan")
            {
                makananItems.Remove(currentItem);
                currentMakanan = (makananItems.Count > 0) ? makananItems[0] : null;
            }
            else if (jenis == "minuman")
            {
                minumanItems.Remove(currentItem);
                currentMinuman = (minumanItems.Count > 0) ? minumanItems[0] : null;
            }
            else if (jenis == "obat")
            {
                obatItems.Remove(currentItem);
                currentObat = (obatItems.Count > 0) ? obatItems[0] : null;
            }
        }

        RefreshUI();
        SaveInventoryData(); // Simpan setiap perubahan
        isConsuming = false; // Reset flag setelah update UI
    }

    private void RefreshUI()
    {
        // Makanan
        if (currentMakanan == null)
        {
            if (itemMakanan != null) itemMakanan.gameObject.SetActive(false);
            if (itemMakananCountText != null) itemMakananCountText.gameObject.SetActive(false);
        }
        else
        {
            if (itemMakanan != null) itemMakanan.gameObject.SetActive(true);
            if (itemMakananCountText != null)
            {
                itemMakananCountText.gameObject.SetActive(true);
                itemMakananCountText.text = currentMakanan.count.ToString();
            }
        }

        // Minuman
        if (currentMinuman == null)
        {
            if (itemMinuman != null) itemMinuman.gameObject.SetActive(false);
            if (itemMinumanCountText != null) itemMinumanCountText.gameObject.SetActive(false);
        }
        else
        {
            if (itemMinuman != null) itemMinuman.gameObject.SetActive(true);
            if (itemMinumanCountText != null)
            {
                itemMinumanCountText.gameObject.SetActive(true);
                itemMinumanCountText.text = currentMinuman.count.ToString();
            }
        }

        // Obat (NEW)
        if (currentObat == null)
        {
            if (itemObat != null) itemObat.gameObject.SetActive(false);
            if (itemObatCountText != null) itemObatCountText.gameObject.SetActive(false);
        }
        else
        {
            if (itemObat != null) itemObat.gameObject.SetActive(true);
            if (itemObatCountText != null)
            {
                itemObatCountText.gameObject.SetActive(true);
                itemObatCountText.text = currentObat.count.ToString();
            }
        }
    }

    #region Save / Load
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

        // Convert minumanItems ke ItemData
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

        // Convert obatItems ke ItemData (NEW)
        foreach (Item item in obatItems)
        {
            data.obatItems.Add(new ItemData
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

        // Makanan
        makananItems.Clear();
        if (data != null && data.makananItems != null)
        {
            foreach (ItemData itemData in data.makananItems)
            {
                Item item = new Item(itemData.name, itemData.count, itemData.healthBonus, itemData.staminaBonus);
                makananItems.Add(item);
            }
        }
        currentMakanan = (makananItems.Count > 0) ? makananItems[0] : null;

        // Minuman
        minumanItems.Clear();
        if (data != null && data.minumanItems != null)
        {
            foreach (ItemData itemData in data.minumanItems)
            {
                Item item = new Item(itemData.name, itemData.count, itemData.healthBonus, itemData.staminaBonus);
                minumanItems.Add(item);
            }
        }
        currentMinuman = (minumanItems.Count > 0) ? minumanItems[0] : null;

        // Obat (NEW)
        obatItems.Clear();
        if (data != null && data.obatItems != null)
        {
            foreach (ItemData itemData in data.obatItems)
            {
                Item item = new Item(itemData.name, itemData.count, itemData.healthBonus, itemData.staminaBonus);
                obatItems.Add(item);
            }
        }
        currentObat = (obatItems.Count > 0) ? obatItems[0] : null;
    }
    #endregion
}

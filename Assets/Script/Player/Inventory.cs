using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [Header("UI Slot Tunggal")]
    public Button itemMakanan;
    public TextMeshProUGUI itemCountText;

    private List<Item> items = new List<Item>();
    private Item currentItem;
    private bool isConsuming = false; // Flag untuk mencegah klik ganda

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        itemMakanan.onClick.RemoveAllListeners();
        itemMakanan.onClick.AddListener(OnClickSlot);
        RefreshUI();
    }

    // Tambah item
    public void AddItem(Item itemToAdd)
    {
        foreach (Item item in items)
        {
            if (item.name == itemToAdd.name)
            {
                item.count += itemToAdd.count;
                currentItem = item;
                RefreshUI();
                return;
            }
        }

        items.Add(itemToAdd);
        currentItem = itemToAdd;
        RefreshUI();
    }

    // Makan item
    private void OnClickSlot()
    {
        if (isConsuming || currentItem == null) return; // Cegah klik ganda

        isConsuming = true; // Set flag
        Debug.Log("Klik slot makanan terjadi!");

        currentItem.count--;

        // Tambahkan health dan stamina ke Player
        Player.instance.AddHealth(currentItem.healthBonus);
        Player.instance.AddStamina(currentItem.staminaBonus);

        Debug.Log($"Makan {currentItem.name} (+{currentItem.healthBonus} HP, +{currentItem.staminaBonus} Stamina)");

        if (currentItem.count <= 0)
        {
            items.Remove(currentItem);
            currentItem = null;
        }

        RefreshUI();
        isConsuming = false; // Reset flag setelah update UI
    }

    // Update UI
    private void RefreshUI()
    {
        if (currentItem == null)
        {
            itemMakanan.gameObject.SetActive(false);
            itemCountText.gameObject.SetActive(false);
            return;
        }

        itemMakanan.gameObject.SetActive(true);
        itemCountText.gameObject.SetActive(true);
        itemCountText.text = currentItem.count.ToString();
    }
}
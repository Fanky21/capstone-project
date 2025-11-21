using UnityEngine;
using System.Collections.Generic;

public class InventoryStore : MonoBehaviour
{
    public static InventoryStore instance;

    [Header("Slot Tunggal Inventory")]
    public Inventory codeInventory;

    public List<Item> items = new List<Item>();

    void Awake()
    {
        instance = this;
    }

    public void AddItem(Item itemToAdd)
    {
        // Jika item sudah ada, tambah count
        foreach (Item item in items)
        {
            if (item.name == itemToAdd.name)
            {
                item.count += itemToAdd.count;
                UpdateUI(item);
                return;
            }
        }

        // Kalau item baru → masukkan ke list
        items.Add(itemToAdd);
        AssignToSlot(itemToAdd);
    }

    private void AssignToSlot(Item item)
    {
        codeInventory.assignedItem = item;

        codeInventory.itemMakanan.gameObject.SetActive(true);
        codeInventory.itemCountText.gameObject.SetActive(true);

        UpdateUI(item);
    }

    private void UpdateUI(Item item)
    {
        if (codeInventory.assignedItem == item)
        {
            codeInventory.itemCountText.text = item.count.ToString();
        }
        else
        {
            Debug.LogWarning("Slot UI tidak cocok dengan item yang diupdate!");
        }
    }

    public void EatItem(Item item)
    {
        if (item.count > 0)
        {
            item.count--;

            Debug.Log($"Makan {item.name} (+{item.healthBonus} HP +{item.staminaBonus} Stamina)");

            if (item.count <= 0)
            {
                items.Remove(item);
                ClearSlot();
            }
            else
            {
                UpdateUI(item);
            }
        }
    }

    private void ClearSlot()
    {
        codeInventory.assignedItem = null;
        codeInventory.itemMakanan.gameObject.SetActive(false);
        codeInventory.itemCountText.gameObject.SetActive(false);
    }
}

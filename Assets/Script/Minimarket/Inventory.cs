using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    public Button itemMakanan;
    public TextMeshProUGUI itemCountText;
    [HideInInspector] public Item assignedItem;

    void Start()
    {
        itemMakanan.onClick.AddListener(OnClickSlot);

        if (assignedItem != null)
        {
            itemMakanan.gameObject.SetActive(true);
            itemCountText.gameObject.SetActive(true);
            itemCountText.text = assignedItem.count.ToString();
        }
        else
        {
            itemMakanan.gameObject.SetActive(false);
            itemCountText.gameObject.SetActive(false);
        }
    }

    public void OnClickSlot()
    {
        if (assignedItem != null)
        {
            InventoryStore.instance.EatItem(assignedItem);
        }
    }
}

using UnityEngine;

[System.Serializable]
public class CoreItemDataList
{
    public int itemId;
    public string itemName;
    public Sprite itemImage;
    [TextArea(20, 10)]
    public string itemDescription;
}

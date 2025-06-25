using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    public ItemData itemType;
    public string name;
    public Sprite icon;
    public string description;
    public int maxStack;

    public int itemCount;
    public int itemStatus;
    public GameObject ItemPrefab;
    public int Durability;
    public ItemInstance(ItemData itemData)
    {
        itemType = itemData;
        name = itemData.itemName;
        icon = itemData.itemIcon;
        description = itemData.description;
        maxStack = itemData.maxStack;
        itemStatus = (int)itemData.itemType;
        ItemPrefab = itemData.ItemPrefab;
        Durability = (int)itemData.MaxDurability;
        itemCount = 1;
    }
}
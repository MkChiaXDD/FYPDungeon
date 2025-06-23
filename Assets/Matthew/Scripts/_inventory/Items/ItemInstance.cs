using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    public readonly ItemData itemType;
    public readonly string name;
    public readonly Sprite icon;
    public readonly string description;
    public readonly int maxStack;

    public int itemCount;
    public int itemStatus;

    public ItemInstance(ItemData itemData)
    {
        itemType = itemData;
        name = itemData.itemName;
        icon = itemData.itemIcon;
        description = itemData.description;
        maxStack = itemData.maxStack;
        itemStatus = (int)itemData.itemType;
        itemCount = 1;
    }
}
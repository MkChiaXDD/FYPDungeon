using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item Data", order = 1)]
public class ItemSOData : ScriptableObject
{
    public enum ItemType
    {
        Resource,
        Consumable,
        Equipment
    }

    public string itemName;
    public Sprite icon;

    public int maxStack = 1;
    public ItemType itemType;
    [TextArea] public string description;
}
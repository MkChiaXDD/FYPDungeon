using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item Data", order = 1)]
public class ItemData : ScriptableObject
{
    public enum ItemType { Resource, Food, Tool, Weapon }

    public string itemName;
    public Sprite itemIcon;
    [TextArea] public string description;
    public ItemType itemType;  
    public int MaxDurability;
    public GameObject ItemPrefab;
    public int maxStack = 1;

    public virtual float Use() => 0;
}
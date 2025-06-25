using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Item Data", order = 1)]
public class ItemData : ScriptableObject
{
    public enum ItemType { Resource, Food, Tool }

    public Sprite itemIcon;
    public string itemName;
    [TextArea] public string description;
    public int maxStack = 1;
    public ItemType itemType;
    public GameObject ItemPrefab;
    public int MaxDurability;
    public virtual float Use() => 0;
}
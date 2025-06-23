// Inventory.cs
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public const int HotbarSize = 7;
    public int maxSlots = 28;
    public ItemInstance[] items;

    public InventoryManager manager;

    private void Awake() => items = new ItemInstance[maxSlots];

    public void SetManager(InventoryManager manager) => this.manager = manager;

    public bool AddItem(ItemInstance newItem, int amount)
    {
        // Stack on existing items first
        amount = StackToExistingItems(newItem, amount);
        if (amount <= 0) return true;

        // Add to empty slots
        return AddToEmptySlots(newItem, amount);
    }

    private int StackToExistingItems(ItemInstance newItem, int amount)
    {
        for (int i = 0; i < items.Length && amount > 0; i++)
        {
            if (items[i] == null || items[i].itemType != newItem.itemType) continue;

            int availableSpace = items[i].itemType.maxStack - items[i].itemCount;
            if (availableSpace <= 0) continue;

            int addAmount = Mathf.Min(availableSpace, amount);
            items[i].itemCount += addAmount;
            amount -= addAmount;
            manager?.RefreshSlot(i);
        }
        return amount;
    }

    private bool AddToEmptySlots(ItemInstance newItem, int amount)
    {
        for (int i = 0; i < items.Length && amount > 0; i++)
        {
            if (items[i] != null) continue;

            int addAmount = Mathf.Min(amount, newItem.itemType.maxStack);
            items[i] = new ItemInstance(newItem.itemType, addAmount);
            amount -= addAmount;
            manager?.RefreshSlot(i);
        }
        return amount <= 0;
    }
}
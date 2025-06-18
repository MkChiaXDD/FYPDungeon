using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 28;
    public ItemInstance[] items;

    [HideInInspector] public InventoryManager manager;

    private void Awake() => items = new ItemInstance[maxSlots];

    public void SetManager(InventoryManager manager) => this.manager = manager;
    public bool AddItem(ItemInstance newItem, int amount)
    {
        // Try stack existing items first
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemType == newItem.itemType)
            {
                int availableSpace = items[i].itemType.maxStack - items[i].itemCount;

                if (availableSpace > 0)
                {
                    int addAmount = Mathf.Min(availableSpace, amount);
                    items[i].itemCount += addAmount;
                    amount -= addAmount;

                    if (amount <= 0) return true;
                }
            }
        }

        // Add to empty slots
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = new ItemInstance(newItem.itemType);
                items[i].itemCount = Mathf.Min(amount, newItem.itemType.maxStack);
                amount -= items[i].itemCount;

                if (amount <= 0) return true;
            }
        }

        return amount <= 0;
    }
}
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int maxItemSlots = 28;
    public int hotbarSize = 7;
    public ItemInstance[] items;
    public InventoryManager manager;
    public ItemInstance equippedSlot;
    public int equippedSlotNum;

    private void Awake()
    {
        items = new ItemInstance[maxItemSlots];
    }

    public bool AddItem(ItemInstance newItem, int amt)
    {
        // Try stacking first
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].itemType == newItem.itemType)
            {
                int spaceLeft = items[i].maxStack - items[i].itemCount;
                if (spaceLeft > 0)
                {
                    int addAmount = Mathf.Min(spaceLeft, amt);
                    items[i].itemCount += addAmount;
                    amt -= addAmount;
                    manager.UpdateAllCount();
                    if (amt <= 0) return true;
                }
            }
        }

        // Add to empty slots
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = new ItemInstance(newItem.itemType) { itemCount = amt };
                manager.UpdateInventory();
                return true;
            }
        }
        return false;
    }

    public void RemoveItem(ItemData itemToRemove, int amt)
    {
        int remaining = amt;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null || items[i].itemType != itemToRemove) continue;

            if (items[i].itemCount > remaining)
            {
                items[i].itemCount -= remaining;
                manager.UpdateAllCount();
                return;
            }
            else
            {
                remaining -= items[i].itemCount;
                items[i] = null;
                manager.UpdateInventoryUI();
                if (remaining <= 0) return;
            }
        }
    }

    public void RemoveItemAtSlot(int slot, int amt)
    {
        if (items[slot].itemCount > amt)
        {
            items[slot].itemCount -= amt;
            manager.UpdateAllCount();
        }
        else
        {
            items[slot] = null;
            manager.UpdateInventoryUI();
        }
    }

    public ItemInstance GetItem(int num) => items[num];

    public int CheckItemCount(ItemData itemType)
    {
        int count = 0;
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (items[i] != null && items[i].itemType == itemType)
            {
                count += items[i].itemCount;
            }
        }
        return count;
    }

    public void SetManager(InventoryManager newManager) => manager = newManager;

    private void Update()
    {
        for (int i = 0; i < 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < hotbarSize)
            {
                equippedSlot = items[i];
                equippedSlotNum = i;
                manager.HighlightEquippedSlot(i);
            }
        }
    }
}
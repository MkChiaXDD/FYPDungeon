using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public int maxItemSlots = 7;
    public int hotbarSize = 7;
    public List<ItemInstance> items = new List<ItemInstance>();
    public InventoryManager manager;
    public ItemInstance equippedSlot;
    public int equippedSlotNum = 0;
    public UnityEvent ChangeSlot;
    public UnityEvent ChangeDurability;
    private void Awake()
    {
        PopulateList();
    }
    private void Update()
    {
        //equippedSlotNum = 0;
        if (GamStates.instance.State == GamStates.GameState.Paused)
        {
            return;
        }

        SelectSlot();
        
    }
 
    public void RemoveItem(ItemInstance itemToRemove, int amt)
    {
        int remaining = amt;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null || items[i] != itemToRemove) continue;

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
            ChangeSlot?.Invoke();
        }
    }
    public ItemInstance GetItem(int num) {       
        return items[num];
     }
    public int CheckItemCount(ItemInstance itemType)
    {
        int count = 0;
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (items[i] != null && items[i] == itemType)
            {
                count += items[i].itemCount;
            }
        }
        return count;
    }
    public void BreakItem(int itemSlot, int DurabilityUsage = 1)
    {
        if ((items[itemSlot].Durability - DurabilityUsage) > 0)
        {
            items[itemSlot].Durability -= DurabilityUsage;
            ChangeDurability?.Invoke();
        }
        else {
            Debug.Log("Breaking " + items[itemSlot].name);       
            RemoveItemAtSlot(itemSlot, 1); 
        }
    }

    public int GetItemDurability()
    {
        return items[equippedSlotNum].Durability;
    }

    private void SelectSlot()
    {
        // number keyyboard press
        for (int i = 0; i < 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < hotbarSize)
            {
                ChangeEquippedSlot(i);
                return;
            }
        }

        // scrollwheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // Scroll up
        {
            equippedSlotNum--;
            if (equippedSlotNum < 0) equippedSlotNum = hotbarSize - 1;
            ChangeEquippedSlot(equippedSlotNum);
        }
        else if (scroll < 0f) // Scroll down
        {
            equippedSlotNum++;
            if (equippedSlotNum >= hotbarSize) equippedSlotNum = 0;
            ChangeEquippedSlot(equippedSlotNum);
        }
    }

    public bool Pickup(ItemInstance itemToPickup, int amount = 1)
    {
        // check for empty
        if (itemToPickup == null || amount <= 0)
        {
            Debug.LogWarning("Attempted to pickup invalid item or amount");
            return false;
        }

        // check can fit
        if (!CanFitItem(itemToPickup, amount))
        {
            Debug.Log("Inventory full, cannot pickup item");
            return false;
        }

        // Add the item to inventory
        bool success = manager.AddItem(itemToPickup, amount);

        if (success)
        {
            // Update UI
            manager.UpdateInventory();
            manager.UpdateAllCount();

            // Play pickup sound if available
            SoundManager.Instance.PlaySFX("PickupSword");
        }
        return success;
    }

    private bool CanFitItem(ItemInstance item, int amount)
    {
        if (item == null) return false;

        int remaining = amount;

        // First check existing stacks
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (items[i] != null && items[i].itemType == item.itemType)
            {
                int spaceAvailable = items[i].maxStack - items[i].itemCount;
                remaining -= Mathf.Min(spaceAvailable, remaining);

                if (remaining <= 0) return true;
            }
        }

        // Then check empty slots
        int emptySlots = 0;
        for (int i = 0; i < maxItemSlots; i++)
        {
            if (items[i] == null)
            {
                emptySlots++;
                // Each empty slot can hold up to item.maxStack
                remaining -= item.maxStack;

                if (remaining <= 0) return true;
            }
        }

        return remaining <= 0;
    }

    private void ChangeEquippedSlot(int slotIndex)
    {
        SoundManager.Instance.PlaySFX("SelectSFX");
        equippedSlotNum = slotIndex;
        equippedSlot = items[equippedSlotNum];
        manager.HighlightEquippedSlot(equippedSlotNum);
        ChangeSlot.Invoke();
    }

    private void PopulateList()
    {
        // Initialize the items list with null entries up to maxItemSlots
        while (items.Count < maxItemSlots)
        {
            items.Add(null);
        }
    }

    public void SetManager(InventoryManager newManager) => manager = newManager;
}
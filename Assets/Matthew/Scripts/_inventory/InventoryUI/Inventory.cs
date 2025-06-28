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

    private void Awake()
    {
        PopulateList();
    }
    private void Update()
    {
        //equippedSlotNum = 0;
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
    public ItemInstance GetItem(int num) => items[num];
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
        }
        else {
            Debug.Log("Breaking " + items[itemSlot].name);
            RemoveItemAtSlot(itemSlot, 1); 

        }
    }
    private void SelectSlot()
    {
        for (int i = 0; i < 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i < hotbarSize)
            {
                equippedSlot = items[i];
                equippedSlotNum = i;
                manager.HighlightEquippedSlot(i);
                ChangeSlot.Invoke();
            }
        }
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
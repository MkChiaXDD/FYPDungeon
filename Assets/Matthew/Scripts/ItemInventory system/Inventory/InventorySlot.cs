using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    private InventoryManager manager;
    private int slotIndex;

    public void SetManager(InventoryManager manager, int index)
    {
        this.manager = manager;
        slotIndex = index;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem draggedItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (draggedItem == null) return;

        HandleItemDropped(draggedItem);
    }

    public void HandleItemDropped(InventoryItem item)
    {
        // Get source slot from the item's original parent
        InventorySlot sourceSlot = item.OriginalParent.GetComponent<InventorySlot>();

        if (sourceSlot == null) return;
        if (sourceSlot.slotIndex == slotIndex) return; // Same slot

        if (transform.childCount == 0)
        {
            HandleDropToEmptySlot(item, sourceSlot);
        }
        else
        {
            HandleStackMergeOrSwap(item, sourceSlot);
        }
    }

    private void HandleDropToEmptySlot(InventoryItem item, InventorySlot sourceSlot)
    {
        // Move item visually
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;

        // Update inventory data
        manager.UpdateSlotData(slotIndex, item.itemInstance);
        manager.UpdateSlotData(sourceSlot.slotIndex, null);

        // Update item's original parent reference
        item.OriginalParent = transform;
    }

    private void HandleStackMergeOrSwap(InventoryItem incomingItem, InventorySlot sourceSlot)
    {
        InventoryItem currentItem = transform.GetChild(0).GetComponent<InventoryItem>();

        if (currentItem.itemInstance.itemType == incomingItem.itemInstance.itemType)
        {
            MergeStacks(currentItem, incomingItem, sourceSlot);
        }
        else
        {
            SwapItems(currentItem, incomingItem, sourceSlot);
        }
    }

    private void MergeStacks(InventoryItem target, InventoryItem source, InventorySlot sourceSlot)
    {
        int total = target.itemInstance.itemCount + source.itemInstance.itemCount;
        int maxStack = target.itemInstance.itemType.maxStack;

        if (total <= maxStack)
        {
            // Full merge
            target.itemInstance.itemCount = total;
            target.RefreshUI();

            // Update inventory data
            manager.UpdateSlotData(slotIndex, target.itemInstance);
            manager.UpdateSlotData(sourceSlot.slotIndex, null);

            // Remove source item
            Destroy(source.gameObject);
        }
        else
        {
            // Partial merge
            int overflow = total - maxStack;
            target.itemInstance.itemCount = maxStack;
            source.itemInstance.itemCount = overflow;

            // Update both items
            target.RefreshUI();
            source.RefreshUI();

            // Update inventory data
            manager.UpdateSlotData(slotIndex, target.itemInstance);
            manager.UpdateSlotData(sourceSlot.slotIndex, source.itemInstance);
        }
    }

    private void SwapItems(InventoryItem current, InventoryItem incoming, InventorySlot sourceSlot)
    {
        // Swap parents
        Transform tempParent = current.transform.parent;
        current.transform.SetParent(sourceSlot.transform);
        incoming.transform.SetParent(transform);

        // Reset positions
        current.transform.localPosition = Vector3.zero;
        incoming.transform.localPosition = Vector3.zero;

        // Update original parents
        current.OriginalParent = sourceSlot.transform;
        incoming.OriginalParent = transform;

        // Update inventory data
        manager.UpdateSlotData(slotIndex, incoming.itemInstance);
        manager.UpdateSlotData(sourceSlot.slotIndex, current.itemInstance);
    }

    public InventoryManager GetManager() => manager;
}
// InventorySlot.cs
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

    public void OnDrop(PointerEventData eventData) =>
        HandleItemDropped(eventData.pointerDrag.GetComponent<InventoryItem>());

    public void HandleItemDropped(InventoryItem item)
    {
        if (transform.childCount == 0)
        {
            HandleDropToEmptySlot(item);
        }
        else
        {
            HandleStackMerge(item);
        }
    }

    private void HandleDropToEmptySlot(InventoryItem item)
    {
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
        manager.UpdateSlotData(slotIndex, item.itemInstance);
    }

    private void HandleStackMerge(InventoryItem incomingItem)
    {
        InventoryItem currentItem = transform.GetChild(0).GetComponent<InventoryItem>();

        if (currentItem.itemInstance.itemType == incomingItem.itemInstance.itemType)
        {
            MergeStacks(currentItem, incomingItem);
        }
        else
        {
            SwapItems(currentItem, incomingItem);
        }
    }

    private void MergeStacks(InventoryItem target, InventoryItem source)
    {
        int total = target.itemInstance.itemCount + source.itemInstance.itemCount;
        int maxStack = target.itemInstance.itemType.maxStack;

        if (total <= maxStack)
        {
            target.itemInstance.itemCount = total;
            Destroy(source.gameObject);
            manager.UpdateSlotData(slotIndex, target.itemInstance);
        }
        else
        {
            target.itemInstance.itemCount = maxStack;
            source.itemInstance.itemCount = total - maxStack;
            manager.UpdateSlotData(slotIndex, target.itemInstance);
        }
        target.RefreshUI();
    }

    private void SwapItems(InventoryItem current, InventoryItem incoming)
    {
        Transform currentParent = incoming.transform.parent;
        InventorySlot currentSlot = currentParent.GetComponent<InventorySlot>();

        incoming.transform.SetParent(transform);
        incoming.transform.localPosition = Vector3.zero;
        current.transform.SetParent(currentParent);
        current.transform.localPosition = Vector3.zero;

        manager.UpdateSlotData(slotIndex, incoming.itemInstance);
        manager.UpdateSlotData(currentSlot.slotIndex, current.itemInstance);
    }

    public InventoryManager GetManager() => manager;
}
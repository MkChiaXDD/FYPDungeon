using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    private InventoryManager manager;

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            HandleDropToEmptySlot(eventData);
        }
        else
        {
            HandleStackMerge(eventData);
        }
    }

    private void HandleDropToEmptySlot(PointerEventData eventData)
    {
        InventoryItem item = eventData.pointerDrag.GetComponent<InventoryItem>();
        item.parentAfterDrag = transform;
    }

    private void HandleStackMerge(PointerEventData eventData)
    {
        InventoryItem child = transform.GetChild(0).GetComponent<InventoryItem>();
        InventoryItem heldItem = eventData.pointerDrag.GetComponent<InventoryItem>();

        if (child.itemInstance.itemType == heldItem.itemInstance.itemType)
        {
            MergeStacks(child, heldItem);
        }
        else
        {
            SwapItems(heldItem, child);
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
            target.UpdateCount();
        }
        else
        {
            target.itemInstance.itemCount = maxStack;
            source.itemInstance.itemCount = total - maxStack;
            target.UpdateCount();
            source.UpdateCount();
        }
        CallUpdate();
    }

    private void SwapItems(InventoryItem draggedItem, InventoryItem existingItem)
    {
        Transform tempSlot = draggedItem.parentAfterDrag;

        draggedItem.parentAfterDrag = transform;
        existingItem.parentAfterDrag = tempSlot;

        existingItem.UpdateLocation();
    }

    public InventoryManager GetManager() => manager;
    public void SetManager(InventoryManager manager) => this.manager = manager;
    public void CallUpdate() => manager.UpdateSlot();
}
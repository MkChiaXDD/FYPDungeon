using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        InventoryItemUI droppedItem = eventData.pointerDrag.GetComponent<InventoryItemUI>();
        if (!droppedItem) return;

        HandleDroppedItem(droppedItem);
    }

    private void HandleDroppedItem(InventoryItemUI item)
    {
        // Handle different drop scenarios
        if (transform.childCount == 0)
        {
            AcceptDroppedItem(item);
        }
        else
        {
            InventoryItemUI existingItem = GetComponentInChildren<InventoryItemUI>();
            TryMergeOrSwap(item, existingItem);
        }
    }

    private void AcceptDroppedItem(InventoryItemUI item)
    {
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
        InventoryManager.Instance.MoveItemToSlot(item, this);
    }

    private void TryMergeOrSwap(InventoryItemUI incoming, InventoryItemUI existing)
    {
        // Same item type - try to merge
        if (incoming.Instance.itemData == existing.Instance.itemData)
        {
            MergeStacks(incoming, existing);
        }
        else
        {
            SwapItems(incoming, existing);
        }
    }

    private void MergeStacks(InventoryItemUI source, InventoryItemUI target)
    {
        int total = source.Instance.count + target.Instance.count;
        int maxStack = target.Instance.itemData.maxStack;

        if (total <= maxStack)
        {
            target.Instance.count = total;
            target.UpdateUI();
            Destroy(source.gameObject);
        }
        else
        {
            target.Instance.count = maxStack;
            source.Instance.count = total - maxStack;
            target.UpdateUI();
            source.UpdateUI();
        }
    }

    private void SwapItems(InventoryItemUI item1, InventoryItemUI item2)
    {
        Transform tempParent = item1.originalParent;
        item1.transform.SetParent(item2.originalParent);
        item2.transform.SetParent(tempParent);

        item1.ReturnToOriginalParent();
        item2.ReturnToOriginalParent();

        InventoryManager.Instance.SwapItems(item1, item2);
    }
}
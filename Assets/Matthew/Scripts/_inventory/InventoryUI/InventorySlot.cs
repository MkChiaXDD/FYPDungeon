using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    private InventoryManager manager;
    private Image slotImage;

    private void Awake() => slotImage = GetComponent<Image>();

    public void SetManager(InventoryManager newManager) => manager = newManager;
    public InventoryManager GetManager() => manager;

    public void OnDrop(PointerEventData eventData)
    {
        InventoryItem draggedItem = eventData.pointerDrag.GetComponent<InventoryItem>();
        if (transform.childCount == 0)
        {
            draggedItem.parentAfterDrag = transform;
            manager.UpdateSlot();
        }
        else
        {
            InventoryItem existingItem = transform.GetChild(0).GetComponent<InventoryItem>();
            if (existingItem.itemInstance.itemType == draggedItem.itemInstance.itemType)
            {
                HandleStackableMerge(draggedItem, existingItem);
            }
            else
            {
                SwapItems(draggedItem, existingItem);
            }
        }
    }

    private void HandleStackableMerge(InventoryItem dragged, InventoryItem existing)
    {
        int total = existing.itemInstance.itemCount + dragged.itemInstance.itemCount;
        if (total <= existing.itemInstance.maxStack)
        {
            existing.itemInstance.itemCount = total;
            Destroy(dragged.gameObject);
            existing.UpdateCount();
        }
        else
        {
            int overflow = total - existing.itemInstance.maxStack;
            existing.itemInstance.itemCount = existing.itemInstance.maxStack;
            dragged.itemInstance.itemCount = overflow;
            dragged.UpdateCount();
            existing.UpdateCount();
        }
        manager.UpdateSlot(); 


    }

    private void SwapItems(InventoryItem dragged, InventoryItem existing)
    {
        Transform tempParent = dragged.parentAfterDrag;
        dragged.parentAfterDrag = transform;
        existing.parentAfterDrag = tempParent;

        dragged.UpdateLocation();
        existing.UpdateLocation();

        manager.UpdateSlot();
    }

    public void CallUpdate() => manager.UpdateSlot();
}
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text countText;

    public Transform parentAfterDrag;
    public ItemInstance itemInstance;

    private void Start() => RefreshUI();

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData) => transform.position = Input.mousePosition;

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
        parentAfterDrag.GetComponent<InventorySlot>().CallUpdate();
        RefreshUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemInstance.itemCount > 1)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        int halfCount = itemInstance.itemCount / 2;
        itemInstance.itemCount -= halfCount;
        RefreshUI();

        // Get manager through the slot
        InventorySlot slot = parentAfterDrag.GetComponent<InventorySlot>();
        if (slot == null) return;

        InventoryManager manager = slot.GetManager();
        if (manager == null) return;

        // Create new item instance
        ItemInstance newItem = new ItemInstance(itemInstance.itemType);
        newItem.itemCount = halfCount;

        // Add to inventory
        manager.AddItemToSlot(newItem);
    }

    public void Initialize(ItemInstance item, int count)
    {
        itemInstance = item;
        itemInstance.itemCount = count;
        itemInstance.itemType.icon = item.itemType.icon;
        RefreshUI();
    }

    // Made public and renamed to UpdateCount for consistency
    public void UpdateCount()
    {
        if (itemInstance != null)
        {
            image.sprite = itemInstance.itemType.icon;
            countText.text = itemInstance.itemCount > 1 ? itemInstance.itemCount.ToString() : "";
        }
    }

    // Alias for UpdateCount
    public void RefreshUI() => UpdateCount();

    public void UpdateLocation()
    {
        transform.SetParent(parentAfterDrag);
        UpdateCount();
    }
}
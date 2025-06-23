// InventoryItem.cs
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text countText;

    private Transform parentAfterDrag;
    public ItemInstance itemInstance { get; private set; }

    public void Initialize(ItemInstance item)
    {
        itemInstance = item;
        RefreshUI();
    }

    public void RefreshUI()
    {
        image.sprite = itemInstance.itemType.icon;
        countText.text = itemInstance.itemCount > 1 ? itemInstance.itemCount.ToString() : "";
    }

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

        // Only reset position if not dropped on a valid slot
        if (transform.parent == transform.root)
        {
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            itemInstance.itemCount > 1)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        InventorySlot currentSlot = transform.parent.GetComponent<InventorySlot>();
        if (currentSlot == null) return;

        int halfCount = itemInstance.itemCount / 2;
        itemInstance.itemCount -= halfCount;
        RefreshUI();

        ItemInstance newItem = new ItemInstance(itemInstance.itemType, halfCount);
        currentSlot.GetManager().AddItemToSlot(newItem);
    }
}
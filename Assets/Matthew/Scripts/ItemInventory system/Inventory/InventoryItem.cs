using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text countText;

    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    public Transform OriginalParent { get; set; }
    public ItemInstance itemInstance { get; private set; }

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (parentCanvas == null)
        {
            parentCanvas = FindObjectOfType<Canvas>();
        }

        // Store original parent on awake
        if (OriginalParent == null)
            OriginalParent = transform.parent;
    }

    public void Initialize(ItemInstance item)
    {
        itemInstance = item;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (itemInstance == null || itemInstance.itemType == null) return;

        image.sprite = itemInstance.itemType.icon;
        countText.text = itemInstance.itemCount > 1 ?
            itemInstance.itemCount.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemInstance == null) return;

        // Disable raycast blocking during drag
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            image.raycastTarget = false;
        }

        // Store original parent if not set
        if (OriginalParent == null)
            OriginalParent = transform.parent;

        // Move to canvas root
        transform.SetParent(parentCanvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Re-enable raycast blocking
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            image.raycastTarget = true;
        }

        // If not dropped on a valid slot, return to original position
        if (transform.parent == parentCanvas.transform)
        {
            transform.SetParent(OriginalParent);
            transform.localPosition = Vector3.zero;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            itemInstance != null &&
            itemInstance.itemCount > 1)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        InventorySlot currentSlot = OriginalParent.GetComponent<InventorySlot>();
        if (currentSlot == null) return;

        int halfCount = itemInstance.itemCount / 2;
        itemInstance.itemCount -= halfCount;
        RefreshUI();

        ItemInstance newItem = new ItemInstance(itemInstance.itemType, halfCount);
        currentSlot.GetManager().AddItemToSlot(newItem);
    }
}
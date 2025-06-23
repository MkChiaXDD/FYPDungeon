using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject descriptionBox;

    private Image itemImage;
    private TMP_Text descriptionText;
    public ItemInstance itemInstance;
    public Transform parentAfterDrag;

    private void Awake()
    {
        itemImage = GetComponent<Image>();
        descriptionText = descriptionBox.GetComponentInChildren<TMP_Text>();
        descriptionBox.SetActive(false);
    }

    public void ObtainItem(ItemInstance newItem, int amt)
    {
        itemInstance = newItem;
        itemInstance.itemCount = amt;
        itemImage.sprite = itemInstance.icon;
        UpdateCount();
    }

    public ItemInstance GetItem() => itemInstance;

    public void UpdateCount() => countText.text = itemInstance.itemCount.ToString();

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemImage.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        UpdateCount();
    }

    public void OnDrag(PointerEventData eventData) => transform.position = Input.mousePosition;

    public void OnEndDrag(PointerEventData eventData)
    {
        itemImage.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
        parentAfterDrag.GetComponent<InventorySlot>().CallUpdate();
        UpdateCount();
    }

    public void UpdateLocation()
    {
        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;
        parentAfterDrag.GetComponent<InventorySlot>().CallUpdate();
        UpdateCount();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemInstance.itemCount < 2) return;

        itemInstance.itemCount--;
        UpdateCount();
        parentAfterDrag.GetComponent<InventorySlot>().GetManager().AddItem(itemInstance, 1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        descriptionBox.SetActive(true);
        descriptionBox.transform.position = Input.mousePosition;
        descriptionText.text = $"{itemInstance.name}\n{itemInstance.description}";
        descriptionBox.transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData) => descriptionBox.SetActive(false);
}
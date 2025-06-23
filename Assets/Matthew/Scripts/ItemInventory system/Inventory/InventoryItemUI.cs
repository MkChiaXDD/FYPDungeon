using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    private CanvasGroup canvasGroup;
    public Transform originalParent;
    public ItemInstance Instance { get; private set; }

    private void Awake() => canvasGroup = GetComponent<CanvasGroup>();

    public void Setup(ItemInstance item, Transform parent)
    {
        Instance = item;
        originalParent = parent;
        transform.SetParent(parent);
        UpdateUI();
    }

    public void UpdateUI()
    {
        iconImage.sprite = Instance.itemData.itemSprite;
        countText.text = Instance.count > 1 ? Instance.count.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData) =>
        transform.position = eventData.position;

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        if (transform.parent == transform.root)
            ReturnToOriginalParent();
    }

    public void ReturnToOriginalParent()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left &&
            Instance.count > 1)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        int halfCount = Instance.count / 2;
        Instance.count -= halfCount;
        UpdateUI();

        InventoryManager.Instance.AddToFirstEmptySlot(
            new ItemInstance(Instance.itemData, halfCount)
        );
    }
}
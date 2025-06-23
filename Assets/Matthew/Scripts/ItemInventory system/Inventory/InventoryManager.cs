using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Settings")]
    [SerializeField] private int totalSlots = 28;
    [SerializeField] private int hotbarSize = 7;

    [Header("UI References")]
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemPrefab;

    private ItemInstance[] items;
    private bool isInventoryOpen;

    private void Awake()
    {
        Instance = this;
        items = new ItemInstance[totalSlots];
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Create UI slots
        for (int i = 0; i < totalSlots; i++)
        {
            Transform parent = i < hotbarSize ? hotbarContainer : inventoryContainer;
            InstantiateSlot(parent);
        }
    }

    private void InstantiateSlot(Transform parent)
    {
        GameObject slot = new GameObject("Slot");
        slot.transform.SetParent(parent);
        slot.AddComponent<InventorySlot>();
        slot.AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isInventoryOpen);
        Time.timeScale = isInventoryOpen ? 0 : 1;
        Cursor.visible = isInventoryOpen;
    }

    public void AddToFirstEmptySlot(ItemInstance item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                AddItemToSlot(item, i);
                return;
            }
        }
        Debug.Log("Inventory full!");
    }

    public void AddItemToSlot(ItemInstance item, int slotIndex)
    {
        items[slotIndex] = item;
        CreateItemUI(item, GetSlotTransform(slotIndex));
    }

    private Transform GetSlotTransform(int index)
    {
        Transform parent = index < hotbarSize ? hotbarContainer : inventoryContainer;
        return parent.GetChild(index % hotbarSize);
    }

    private void CreateItemUI(ItemInstance item, Transform slot)
    {
        GameObject itemObj = Instantiate(itemPrefab, slot);
        itemObj.GetComponent<InventoryItemUI>().Setup(item, slot);
    }

    // Called when items are moved between slots
    public void MoveItemToSlot(InventoryItemUI item, InventorySlot newSlot)
    {
        // Update your data model here
    }

    // Called when items are swapped
    public void SwapItems(InventoryItemUI item1, InventoryItemUI item2)
    {
        // Update your data model here
    }
}
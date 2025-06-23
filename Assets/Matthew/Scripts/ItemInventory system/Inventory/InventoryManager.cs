// InventoryManager.cs
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Inventory inventory;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject itemPrefab;

    [Header("UI References")]
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventoryPanel;

    private InventorySlot[] slots;

    private void Awake()
    {
        InitializeSlots();
        inventory.SetManager(this);
        GetComponentInParent<Canvas>().enabled = true;
    }

    private void InitializeSlots()
    {
        slots = new InventorySlot[inventory.maxSlots];

        for (int i = 0; i < inventory.maxSlots; i++)
        {
            Transform parent = i < Inventory.HotbarSize ? hotbarContainer : inventoryContainer;
            slots[i] = CreateSlot(parent, i);
        }
        RefreshInventory();
    }

    private InventorySlot CreateSlot(Transform parent, int index)
    {
        GameObject slotObj = Instantiate(slotPrefab, parent);
        InventorySlot slot = slotObj.GetComponent<InventorySlot>();
        slot.SetManager(this, index);
        return slot;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0f : 1f;
        Cursor.visible = isActive;
    }

    public void RefreshSlot(int index)
    {
        ClearSlot(index);
        CreateItemInSlot(index);
    }

    private void ClearSlot(int index)
    {
        if (slots[index].transform.childCount > 0)
        {
            Destroy(slots[index].transform.GetChild(0).gameObject);
        }
    }

    private void CreateItemInSlot(int index)
    {
        if (inventory.items[index] == null) return;

        GameObject itemObj = Instantiate(itemPrefab, slots[index].transform);
        itemObj.GetComponent<InventoryItem>().Initialize(inventory.items[index]);
    }

    public void RefreshInventory()
    {
        for (int i = 0; i < inventory.items.Length; i++)
        {
            RefreshSlot(i);
        }
    }

    public void AddItem(ItemSOData itemType, int amount)
    {
        inventory.AddItem(new ItemInstance(itemType), amount);
    }

    public void UpdateSlotData(int slotIndex, ItemInstance item)
    {
        inventory.items[slotIndex] = item;
    }

    public void AddItemToSlot(ItemInstance itemInstance)
    {
        int emptySlotIndex = FindEmptySlot();
        if (emptySlotIndex == -1)
        {
            Debug.LogWarning("No empty slot found for new item");
            return;
        }

        inventory.items[emptySlotIndex] = itemInstance;
        RefreshSlot(emptySlotIndex);
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (inventory.items[i] == null)
                return i;
        }
        return -1;
    }
}
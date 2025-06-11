using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private int hotbarSize = 7;

    [Header("Prefabs")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject itemPrefab;

    [Header("UI References")]
    [SerializeField] private Transform hotbarContainer;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventoryPanel;

   

    private GameObject[] slots;

    private void Awake()
    {
        InitializeSlots();
        inventory.SetManager(this);
        GetComponentInParent<Canvas>().enabled = true;      
    }

    private void InitializeSlots()
    {
        slots = new GameObject[inventory.maxSlots];

        // Create hotbar slots
        for (int i = 0; i < hotbarSize; i++)
        {
            slots[i] = CreateSlot(hotbarContainer, i);
        }

        // Create inventory slots
        for (int i = hotbarSize; i < inventory.maxSlots; i++)
        {
            slots[i] = CreateSlot(inventoryContainer, i);
        }

        RefreshInventory();
    }

    private GameObject CreateSlot(Transform parent, int index)
    {
        GameObject slot = Instantiate(slotPrefab, parent);
        slot.GetComponent<InventorySlot>().SetManager(this);
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
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        Cursor.lockState = inventoryPanel.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryPanel.activeSelf;
    }

    public void UpdateSlot()
    {
        for (int i = 0; i < inventory.maxSlots; i++)
        {
            inventory.items[i] = slots[i].transform.childCount > 0 ?
                slots[i].transform.GetChild(0).GetComponent<InventoryItem>().itemInstance:null;
        }
    }

    public void RefreshInventory()
    {
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (inventory.items[i] != null && slots[i].transform.childCount == 0)
            {
                CreateItem(inventory.items[i], slots[i].transform);
            }
        }
    }

    private void CreateItem(ItemInstance item, Transform parent)
    {
        GameObject itemObj = Instantiate(itemPrefab, parent);
        Debug.Log("itemname: " + item.ToString());
        Debug.Log("itemCount: " + item.itemCount);
        itemObj.GetComponent<InventoryItem>().Initialize(item, item.itemCount);
    }

    public void AddItem(ItemSOData itemType, int amount)
    {
        inventory.AddItem(new ItemInstance(itemType), amount);
        RefreshInventory();
    }

    // Added missing method
    public void AddItemToSlot(ItemInstance itemInstance)
    {
        int emptySlotIndex = FindEmptySlot();
        if (emptySlotIndex != -1)
        {
            CreateItem(itemInstance, slots[emptySlotIndex].transform);
            UpdateSlot();
        }
    }

    public int FindEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].transform.childCount == 0)
                return i;
        }
        return -1;
    }
}
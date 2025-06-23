using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    private int hotbarSize;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private GameObject hotbarUI;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject inventoryPage;
    [SerializeField] private Sprite normalTex;
    [SerializeField] private Sprite highlightedTex;
    [SerializeField] private Image pickupImage;

    private GameObject[] inventorySlots;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = true;
        inventorySlots = new GameObject[inventory.maxItemSlots];
        hotbarSize = inventory.hotbarSize;

        InitializeSlots();
        inventory.SetManager(this);
    }

    private void InitializeSlots()
    {
        // Create hotbar slots
        for (int i = 0; i < hotbarSize; i++)
        {
            inventorySlots[i] = Instantiate(inventorySlotPrefab, hotbarUI.transform);
            inventorySlots[i].GetComponent<InventorySlot>().SetManager(this);
        }

        // Create inventory slots
        for (int i = hotbarSize; i < inventory.maxItemSlots; i++)
        {
            inventorySlots[i] = Instantiate(inventorySlotPrefab, inventoryUI.transform);
            inventorySlots[i].GetComponent<InventorySlot>().SetManager(this);
        }

        // Populate with existing items
        for (int i = 0; i < inventory.maxItemSlots; i++)
        {
            if (i < inventory.items.Length && inventory.items[i] != null)
            {
                CreateItemInSlot(inventory.items[i], i);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        bool isActive = !inventoryPage.activeInHierarchy;
        inventoryPage.SetActive(isActive);
     //   UpdateCursorState(isActive);
       // AudioManager.Instance.PlaySFX("OpenInventory");
    }

  

    public void UpdateSlot()
    {
        for (int i = 0; i < inventory.maxItemSlots; i++)
        {
            if (inventorySlots[i].transform.childCount > 0)
            {
                inventory.items[i] = inventorySlots[i].transform.GetChild(0).GetComponent<InventoryItem>().GetItem();
            }
            else
            {
                inventory.items[i] = null;
            }
        }
    }

    public void UpdateInventory()
    {
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (inventory.items[i] != null && inventory.items[i].itemType != null &&
                inventorySlots[i].transform.childCount == 0)
            {
                CreateItemInSlot(inventory.items[i], i);
            }
        }
    }

    private void CreateItemInSlot(ItemInstance item, int slotIndex)
    {
        GameObject temp = Instantiate(inventoryItemPrefab, inventorySlots[slotIndex].transform);
        temp.GetComponent<InventoryItem>().ObtainItem(item, item.itemCount);
    }

    public void UpdateInventoryUI()
    {
        for (int i = 0; i < inventory.maxItemSlots; i++)
        {
            if (inventorySlots[i].transform.childCount > 0 &&
                (inventory.items[i] == null || inventory.items[i].itemType == null))
            {
                Destroy(inventorySlots[i].transform.GetChild(0).gameObject);
            }
        }
    }

    public void AddItem(ItemInstance item, int amt)
    {
        for (int i = 0; i < inventory.maxItemSlots; i++)
        {
            if (inventory.items[i] == null || inventory.items[i].itemType == null)
            {
                ItemInstance newItem = new ItemInstance(item.itemType);
                GameObject temp = Instantiate(inventoryItemPrefab, inventorySlots[i].transform);
                temp.GetComponent<InventoryItem>().ObtainItem(newItem, amt);
                break;
            }
        }
        UpdateSlot();
    }

    public void UpdateAllCount()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].transform.childCount > 0)
            {
                inventorySlots[i].transform.GetChild(0).GetComponent<InventoryItem>().UpdateCount();
            }
        }
    }

    /// <summary>
    /// Gets the ItemData from the currently selected hotbar slot
    /// </summary>
    public ItemData GetCurrentHotbarItem()
    {
        // Return null if no valid slot is equipped
        if (inventory.equippedSlotNum < 0 || inventory.equippedSlotNum >= hotbarSize)
            return null;

        // Get item from equipped slot
        ItemInstance equippedItem = inventory.GetItem(inventory.equippedSlotNum);

        // Return the item data if exists, otherwise null
        return equippedItem?.itemType;
    }

    public void HighlightEquippedSlot(int slotIndex)
    {
        for (int i = 0; i < hotbarSize; i++)
        {
            inventorySlots[i].GetComponent<Image>().sprite =
                (i == slotIndex) ? highlightedTex : normalTex;
        }
    }

    public void HandPercentage(float fill, bool visible)
    {
        pickupImage.gameObject.SetActive(visible);
        pickupImage.fillAmount = fill;
    }

    public void InvokeUpdateInventory(float time) => Invoke(nameof(UpdateInventory), time);

    public Inventory GetInventory() => inventory;
}
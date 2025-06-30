using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

    public UnityEvent ModifySlot;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.enabled = true;
        inventorySlots = new GameObject[inventory.maxItemSlots];
        hotbarSize = inventory.hotbarSize;

        InitializeSlots();
        inventory.SetManager(this);
        HighlightEquippedSlot(0);
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
            if (i < inventory.items.Count && inventory.items[i] != null)
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
    }

    public void UpdateSlot()
    {
        for (int i = 0; i < inventory.maxItemSlots; i++)
        {
            if (inventorySlots[i].transform.childCount > 0)
            {
                InventoryItem item = inventorySlots[i].transform.GetChild(0).GetComponent<InventoryItem>();
                if (item != null)
                {
                    inventory.items[i] = item.GetItem();
                }
            }
            else
            {
                inventory.items[i] = null;
            }
        }
        ModifySlot?.Invoke();
    }

    public void UpdateInventory()
    {
        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i] != null && inventorySlots[i].transform.childCount == 0)
            {
                CreateItemInSlot(inventory.items[i], i);
            }
        }
        ModifySlot?.Invoke();
    }

    private void CreateItemInSlot(ItemInstance item, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Length) return;

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

    public bool AddItem(ItemInstance item, int amt)
    {
        if (item == null || amt <= 0) return false;

        Debug.Log("where: " + amt);
        Debug.Log("Item : " + item.name);
        Debug.Log("inventory: " + inventory.maxItemSlots);

        // First try to stack with existing items
        for (int i = 0; i < inventory.maxItemSlots; i++)
        {


            //Debug.Log(inventory.items[i].itemCount);
            //Debug.Log(inventory.items[i].maxStack);


            if (inventory.items[i] != null &&
                inventory.items[i].itemType == item.itemType &&
                inventory.items[i].itemCount < inventory.items[i].maxStack)
            {


                int spaceAvailable = inventory.items[i].maxStack - inventory.items[i].itemCount;
                int toAdd = Mathf.Min(spaceAvailable, amt);

                inventory.items[i].itemCount += toAdd;
                amt -= toAdd;

                // Update UI immediately
                if (inventorySlots[i].transform.childCount > 0)
                {
                    inventorySlots[i].transform.GetChild(0).GetComponent<InventoryItem>().UpdateCount();
                }
                else
                {
                    CreateItemInSlot(inventory.items[i], i);
                }

                if (amt <= 0) return true;
            }
        }

        // Then try empty slots
        if (amt > 0)
        {
            for (int i = 0; i < inventory.maxItemSlots; i++)
            {
                if (inventory.items[i] == null)
                {
                    ItemInstance newItem = new ItemInstance(item.itemType)
                    {
                        itemCount = Mathf.Min(amt, item.maxStack),
                        maxStack = item.maxStack
                    };
                    amt -= newItem.itemCount;

                    inventory.items[i] = newItem;
                    CreateItemInSlot(newItem, i);

                    if (amt <= 0) return true;
                }
            }
        }

        return amt <= 0;
    }
    public void RemoveItem(ItemInstance itemToRemove, int amt)
    {
        int remaining = amt;
        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i] == null || inventory.items[i] != itemToRemove) continue;

            if (inventory.items[i].itemCount > remaining)
            {
                inventory.items[i].itemCount -= remaining;
                remaining = 0;
            }
            else
            {
                remaining -= inventory.items[i].itemCount;
                inventory.items[i] = null;
                if (inventorySlots[i].transform.childCount > 0)
                {
                    Destroy(inventorySlots[i].transform.GetChild(0).gameObject);
                }
            }

            if (remaining <= 0) break;
        }

        UpdateAllCount();
    }

    public void UpdateAllCount()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].transform.childCount > 0)
            {
                if (inventorySlots[i].transform.GetChild(0).TryGetComponent<InventoryItem>(out var item))
                {
                    item.UpdateCount();
                }
            }
        }
    }

    public ItemInstance GetCurrentHotbarItem()
    {
        if (inventory.equippedSlotNum < 0 || inventory.equippedSlotNum >= hotbarSize)
            return null;

        return inventory.GetItem(inventory.equippedSlotNum);
    }

    public void RemoveCurrentHotbarItem()
    {
        if (inventory.equippedSlotNum < 0 || inventory.equippedSlotNum >= hotbarSize)
            return;

        inventory.items[inventory.equippedSlotNum] = null;
        UpdateInventoryUI();
    }

    public List<ItemInstance> GetAllHotbarItems()
    {
        List<ItemInstance> items = new List<ItemInstance>();
        for (int i = 0; i < hotbarSize; i++)
        {
            items.Add(inventory.GetItem(i));
        }
        return items;
    }

    public void HighlightEquippedSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= hotbarSize) return;

        for (int i = 0; i < hotbarSize; i++)
        {
            if (inventorySlots[i].TryGetComponent<Image>(out var slotImage))
            {
                slotImage.sprite = (i == slotIndex) ? highlightedTex : normalTex;
            }
        }
    }

    public void InvokeUpdateInventory(float time) => Invoke(nameof(UpdateInventory), time);

    public Inventory GetInventory() => inventory;
}
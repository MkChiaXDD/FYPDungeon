using UnityEngine;

public class Pickupable : MonoBehaviour
{
    [Header("Item Settings")]
    public ItemSOData itemData;  // Changed from ItemSOData to ItemData
    public int amount = 1;

    [Header("Collection Settings")]
    [SerializeField] private float collectionTime = 0.5f;
    [SerializeField] private GameObject progressUI; // Optional progress indicator

    private float collectionProgress;
    private bool isCollecting;
    private PlayerController player; // Cached reference

    private void Update()
    {
        if (!isCollecting) return;

        collectionProgress += Time.deltaTime;
        UpdateProgressUI();

        if (collectionProgress >= collectionTime)
        {
            CollectItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        player = other.GetComponent<PlayerController>();
        if (player == null) return;

        StartCollection();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        ResetCollection();
    }

    private void StartCollection()
    {
        // Instant collection if no collection time
        if (collectionTime <= 0)
        {
            CollectItem();
            return;
        }

        isCollecting = true;
        collectionProgress = 0;

        if (progressUI != null)
            progressUI.SetActive(true);
    }

    private void ResetCollection()
    {
        isCollecting = false;

        if (progressUI != null)
            progressUI.SetActive(false);
    }

    private void UpdateProgressUI()
    {
        if (progressUI == null) return;
        // Update progress bar or radial fill here
        // Example: progressBar.fillAmount = collectionProgress / collectionTime;
    }

    private void CollectItem()
    {
        if (InventoryManager.Instance != null)
        {
            // Create item instance and add to inventory
            var newItem = new ItemInstance(itemData, amount);
            InventoryManager.Instance.AddToFirstEmptySlot(newItem);
        }
        else
        {
            Debug.LogWarning("Inventory manager not found!");
        }

        Destroy(gameObject);
    }
}
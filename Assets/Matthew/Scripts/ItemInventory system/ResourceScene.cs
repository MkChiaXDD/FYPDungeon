using UnityEngine;

public class Resource : MonoBehaviour
{
    public ItemSOData itemData;
    public int amount = 1;
    public float collectionTime;

    private float collectionProgress;
    private bool isCollecting;

    private void Update()
    {
        if (isCollecting)
        {
            collectionProgress += Time.deltaTime;
            if (collectionProgress >= collectionTime)
            {
                CollectResource();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectionTime <= 0) CollectResource();
            else isCollecting = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isCollecting = false;
    }

    private void CollectResource()
    {
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            inventory.manager.AddItem(itemData, amount);
        }
        Destroy(gameObject);
    }
}
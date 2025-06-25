using UnityEngine;

public class Pickupable : MonoBehaviour
{
    public ItemData drop;
    [SerializeField] private int dropAmt = 1;
    [SerializeField] private float timeToObtain = 0f;

    private float timer;
    private InventoryManager InventoryManager;
    private const string PlayerTag = "Player";

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.transform.CompareTag(PlayerTag)) return;
        if (!collision.transform.GetComponent<Inventory>()) return;
        if (timeToObtain > 0f) return;
        TryPickup(collision.gameObject);
        GetComponent<BoxCollider>().isTrigger = true;
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    if (timeToObtain <= 0f) return;
    //    TryTimedPickup(other.gameObject);
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!other.CompareTag(PlayerTag)) return;

    //    timer = 0;
    //    if (playerInventory != null)
    //    {
    //        playerInventory.manager.HandPercentage(0, false);
    //    }
    //}

    private void TryPickup(GameObject playerObject)
    {
        if (!playerObject.CompareTag(PlayerTag)) return;

        //AudioManager.Instance.PlaySFX("Pickup");
        AddToInventory(FindObjectOfType<InventoryManager>());
        Destroy(gameObject);
    }

    private void TryTimedPickup(GameObject playerObject)
    {
        if (!playerObject.CompareTag(PlayerTag)) return;

        InventoryManager = FindObjectOfType<InventoryManager>();
        if (timer >= timeToObtain)
        {         
            AddToInventory(InventoryManager);
            Destroy(gameObject);
        }
        else
        {
            timer += Time.deltaTime;        
        }
    }

    private void AddToInventory(InventoryManager inventory)
    {
        ItemInstance newDrop = new ItemInstance(drop);
        //TextManager.TextInstance.CreateText(new Vector3(350, 800, 1), "Picked up " + newDrop.name, Color.white);
        inventory.AddItem(newDrop, dropAmt);
        inventory.UpdateInventory();
    }
}
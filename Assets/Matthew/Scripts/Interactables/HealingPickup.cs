
using UnityEngine;

public class HealingPickup : MonoBehaviour
{
    [SerializeField] private float healAmount = 30;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        other.gameObject.GetComponent<PlayerData>().Heal(healAmount);
        Destroy(gameObject);

    }
}

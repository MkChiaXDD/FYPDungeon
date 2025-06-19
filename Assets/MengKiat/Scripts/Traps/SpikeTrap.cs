using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private int damage = 2;
    [SerializeField] private float activateDuration = 3f;
    [SerializeField] private float activeDuration = 1f;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private GameObject spikes;
    private float timer;
    private bool isActivated = false;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (!isActivated)
        {
            if (timer > activateDuration)
            {
                Debug.Log("SPIKETRAP: TRAP ACTIVATED!");
                isActivated = true;
                timer = 0;
            }
            spikes.SetActive(false);
        }
        else
        {
            if (timer > activeDuration)
            {
                Debug.Log("SPIKETRAP: TRAP DEACTIVATED!");
                isActivated = false;
                timer = 0;
            }
            spikes.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActivated) return;

        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 knockbackDir = (other.transform.position - transform.position).normalized;
                knockbackDir.y = 0f; // Optional: prevent vertical launch
                rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);

                if (other.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);
                    Debug.Log("SPIKETRAP: HIT Something");
                }
            }

            Debug.Log("SPIKETRAP: Player took damage and was knocked back!");
        }
    }
}

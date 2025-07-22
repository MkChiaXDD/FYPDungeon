using System;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private int damage = 2;
    [SerializeField] private float timeToActivate = 3f;
    [SerializeField] private float activeDuration = 1f;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private GameObject spikes;
    [SerializeField] private float spikeMoveSpeed = 5f;
    [SerializeField] private float spikeForwardDistance = 1f;

    private float timer;
    private bool isActivated = false;

    private Vector3 initialPos;
    private Vector3 extendedPos;

    private void Start()
    {
        initialPos = spikes.transform.localPosition;
        extendedPos = initialPos + Vector3.forward * spikeForwardDistance;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!isActivated)
        {
            if (timer > timeToActivate)
            {
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySFX("Spike", this.gameObject);

                }
                Debug.Log("SPIKETRAP: TRAP ACTIVATED!");
                isActivated = true;
                timer = 0;
            }

            // Move spikes back (retracted)
            spikes.transform.localPosition = Vector3.MoveTowards(spikes.transform.localPosition, initialPos, spikeMoveSpeed * Time.deltaTime);
        }
        else
        {
            if (timer > activeDuration)
            {
                Debug.Log("SPIKETRAP: TRAP DEACTIVATED!");
                isActivated = false;
                timer = 0;
            }

            // Move spikes forward (extended)
            spikes.transform.localPosition = Vector3.MoveTowards(spikes.transform.localPosition, extendedPos, spikeMoveSpeed * Time.deltaTime);
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
                knockbackDir.y = 0f;
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

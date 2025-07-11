using System.Collections;
using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [SerializeField] private float flyForce;
    [SerializeField] private GameObject UI;
    [SerializeField] private ParticleSystem explosionParticle;
    private Rigidbody rb;
    private bool playerNearby = false;
    private Transform playerTransform;
    private MeshRenderer mesh;

    private bool isExploding = false;
    [SerializeField] private float explodeDuration = 1f;
    [SerializeField] private float explodeRadius = 20f;
    [SerializeField] private float explodeDamage = 20f;
    [SerializeField] private float explodeForce = 10f;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mesh = GetComponent<MeshRenderer>();

        UI.SetActive(false);
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !isExploding)
        {
            Debug.Log("Plaeyer Hit barrel");
            Vector3 dir = (transform.position - playerTransform.position).normalized;
            dir = new Vector3(dir.x, 0, dir.z);
            rb.AddForce(dir * flyForce, ForceMode.Impulse);
            StartCoroutine(Exploding());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerTransform = other.transform;

            UI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            playerTransform = null;

            UI.SetActive(false);
        }
    }

    private IEnumerator Exploding()
    {
        isExploding = true;

        float t = 0f;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = initialScale * 1.2f;

        while (t < explodeDuration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, t / explodeDuration);

            t += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        Explode();
    }

    void Explode()
    {
        if (explosionParticle != null)
        {
            explosionParticle.Play();
        }   

        Collider[] hits = Physics.OverlapSphere(transform.position, explodeRadius);

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.GetComponentInParent<Rigidbody>(); // Changed line
            if (hit.TryGetComponent<IDamageable>(out var dmg) && rb != null)
            {
                Vector3 direction = (hit.transform.position - transform.position).normalized;
                Vector3 knockbackForce = direction * explodeForce;

                rb.velocity = Vector3.zero;
                rb.AddExplosionForce(explodeForce, transform.position, explodeRadius, 0f, ForceMode.Force);

                dmg.TakeDamage(explodeDamage);
            }
        }

        mesh.enabled = false;
        float time = explosionParticle.main.duration;
        Destroy(gameObject, time);
    }

}

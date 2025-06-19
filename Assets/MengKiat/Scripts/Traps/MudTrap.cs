using System.Collections;
using UnityEngine;

public class MudTrap : MonoBehaviour
{
    float originalDrag;
    private Rigidbody playerRb;
    private Coroutine slowCoroutine;
    [SerializeField] private float slowDuration = 3f;
    [SerializeField] private float slowMultiplier = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.gameObject.GetComponentInParent<Rigidbody>();

            if (playerRb != null)
            {
                if (slowCoroutine != null)
                {
                    StopCoroutine(slowCoroutine);
                }
                else
                {
                    originalDrag = playerRb.drag;
                }

                playerRb.drag = originalDrag * slowMultiplier;
                slowCoroutine = StartCoroutine(SlowPlayer(playerRb));
            }
        }
    }

    private IEnumerator SlowPlayer(Rigidbody rb)
    {
        yield return new WaitForSeconds(slowDuration);
        rb.drag = originalDrag;
        slowCoroutine = null;
    }
}

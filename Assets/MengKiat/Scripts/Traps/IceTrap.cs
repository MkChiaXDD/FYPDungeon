using System.Collections;
using UnityEngine;

public class IceTrap : MonoBehaviour
{
    float originalDrag;
    private Rigidbody playerRb;
    private Coroutine slideCoroutine;
    [SerializeField] private float slideDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.gameObject.GetComponentInParent<Rigidbody>();

            if (playerRb != null)
            {
                if (slideCoroutine != null)
                {
                    Debug.Log("IceTrap: Resetting slide timer.");
                    StopCoroutine(slideCoroutine);
                }
                else
                {
                    originalDrag = playerRb.drag;
                    Debug.Log($"IceTrap: Saved original drag: {originalDrag}");
                }

                playerRb.drag = 0;
                Debug.Log("IceTrap: Set player drag to 0 (sliding).");
                slideCoroutine = StartCoroutine(SlidePlayer(playerRb));
            }
            else
            {
                Debug.LogWarning("IceTrap: No Rigidbody found on player object!");
            }
        }
    }

    private IEnumerator SlidePlayer(Rigidbody rb)
    {
        Debug.Log($"IceTrap: Sliding for {slideDuration} seconds...");
        yield return new WaitForSeconds(slideDuration);
        rb.drag = originalDrag;
        slideCoroutine = null;
        Debug.Log("IceTrap: Sliding ended, drag restored.");
    }
}

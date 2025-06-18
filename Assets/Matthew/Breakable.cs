using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private ItemDropSystem dropSystem;
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(nameof(BreakObject));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(nameof(BreakObject));
        }
    }

    // Using OnTriggerEnter (for trigger collisions)
    void OnTriggerEnter(Collider other)
    {
        // Check if the collider's tag is "Enemy"
        if (other.gameObject.CompareTag("EarthShatterAttack"))
        {
            StartCoroutine(nameof(BreakObject));
            Debug.Log("Triggered by EarthShatter!");
        }
    }

    private IEnumerator BreakObject()
    {
        yield return Instantiate(brokenObject, transform.position, Quaternion.Euler(0, 0, 0));
        dropSystem.SpawnDropItem();
        Destroy(gameObject);
    }

    public void TakeDamage(int damage) => Die();
    public void Die() => StartCoroutine(nameof(BreakObject));

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBossOneController : Enemy
{
    private Transform player;

    private enum State { Idle, Dash, SpinShoot, Hop, Roam }
    private State state = State.Idle;

    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashDelay = 0.5f;
    public int dashCount = 3;

    [Header("Spin Shoot Settings")]
    public float spinDuration = 3f;
    public float spinSpeed = 1f;
    public GameObject bulletPrefab;
    public int bulletsPerWave = 12;
    public float shootInterval = 0.3f;

    [Header("Hop Settings")]
    public int hopCount = 3;
    public float hopDuration = 1f;
    public float hopHeight = 5f;

    [Header("Hop Knockback")]
    public float knockbackRadius = 1f;
    public float knockbackForce = 30f;
    public float knockbackUpwards = 5f;

    [Header("Roam Settings")]
    public float roamDuration = 3f;

    [SerializeField] private ScreenShake screenShake;

    [SerializeField] private BoxCollider Collider;
    [SerializeField] private BoxCollider PlsceHolderCollider;
    [SerializeField] private Rigidbody rigidbody;

    private bool isBusy = false;
    private int attackCounter = 0;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        StartCoroutine(BossLoop());
    }

    void Update()
    {
        if (state != State.SpinShoot && player != null)
        {
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir),
                    Time.deltaTime * 10f
                );
        }
    }

    private IEnumerator BossLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => !isBusy);

            if (attackCounter >= 2)
            {
                attackCounter = 0;
                state = State.Roam;
                yield return StartCoroutine(DoRoam());
            }
            else
            {
                state = (State)Random.Range(1, 4);
                attackCounter++;

                switch (state)
                {
                    case State.Dash:
                        yield return StartCoroutine(DoDash());
                        break;
                    case State.SpinShoot:
                        yield return StartCoroutine(DoSpinShoot());
                        break;
                    case State.Hop:
                        yield return StartCoroutine(DoHop());
                        break;
                }
            }
        }
    }

    private IEnumerator DoDash()
    {
        isBusy = true;
        for (int i = 0; i < dashCount; i++)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            dir.Normalize();
            Vector3 target = transform.position + dir * dashDistance;
            target.y = transform.position.y;

            float t = 0f;
            Vector3 start = transform.position;
            while (t < 1f)
            {
                transform.position = Vector3.Lerp(start, target, t);
                t += Time.deltaTime * data.moveSpeed;
                yield return null;
            }
            transform.position = target;
            yield return new WaitForSeconds(dashDelay);
        }
        isBusy = false;
    }

    private IEnumerator DoSpinShoot()
    {
        isBusy = true;
        float timer = 0f;
        float shootTimer = 0f;
        while (timer < spinDuration)
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                shootTimer = 0f;
                for (int i = 0; i < bulletsPerWave; i++)
                {
                    float angle = i * (360f / bulletsPerWave);
                    Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
                    Instantiate(bulletPrefab, transform.position, Quaternion.identity)
                        .GetComponent<MiniBullet>()
                        ?.Initialize(dir, 10f, data.damage);
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        isBusy = false;
    }

    private IEnumerator DoHop()
    {
        // remember where we started
        Vector3 origin = transform.position;

        isBusy = true;
        Collider.isTrigger = true;
        rigidbody.isKinematic = true;
        PlsceHolderCollider.enabled = true;

        // perform the hops
        for (int i = 0; i < hopCount; i++)
        {
            Vector3 startPos = transform.position;
            Vector3 target = player.position;
            target.y = startPos.y;

            float t = 0f;
            while (t < hopDuration)
            {
                float prog = t / hopDuration;
                float height = Mathf.Sin(prog * Mathf.PI) * hopHeight;
                Vector3 flat = Vector3.Lerp(startPos, target, prog);
                transform.position = new Vector3(flat.x, startPos.y + height, flat.z);

                t += Time.deltaTime * data.moveSpeed;
                yield return null;
            }

            transform.position = target;
            screenShake.Shake();
            ApplyKnockback();
            yield return new WaitForSeconds(dashDelay);
        }

        // now jump back to where we began
        float r = 0f;
        Vector3 returnStart = transform.position;
        while (r < hopDuration)
        {
            float prog = r / hopDuration;
            float height = Mathf.Sin(prog * Mathf.PI) * hopHeight;
            Vector3 flat = Vector3.Lerp(returnStart, origin, prog);
            transform.position = new Vector3(flat.x, origin.y + height, flat.z);

            r += Time.deltaTime * data.moveSpeed;
            yield return null;
        }
        transform.position = origin;

        // cleanup
        Collider.isTrigger = false;
        rigidbody.isKinematic = false;
        PlsceHolderCollider.enabled = false;
        isBusy = false;
    }


    private void ApplyKnockback()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, knockbackRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") && hit.attachedRigidbody != null)
            {
                Rigidbody rb = hit.attachedRigidbody;
                rb.velocity = Vector3.zero;
                Vector3 rnd = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;
                Vector3 knock = rnd * knockbackForce + Vector3.up * knockbackUpwards;
                rb.AddForce(knock, ForceMode.VelocityChange);
            }
        }
    }

    private IEnumerator DoRoam()
    {
        isBusy = true;
        float timer = 0f;
        Vector3 roamDir = Random.insideUnitSphere;
        roamDir.y = 0;
        roamDir.Normalize();
        while (timer < roamDuration)
        {
            transform.position += roamDir * data.moveSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }
        isBusy = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, knockbackRadius);
    }
}

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
    public float hopHeight = 5f;

    [Header("Roam Settings")]
    public float roamDuration = 3f;

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
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
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
                continue;
            }

            state = GetRandomAttack();
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

    private State GetRandomAttack()
    {
        return (State)Random.Range(1, 4); // skip Idle and Roam
    }

    private IEnumerator DoDash()
    {
        isBusy = true;
        state = State.Dash;

        for (int i = 0; i < dashCount; i++)
        {
            Vector3 flatDir = player.position - transform.position;
            flatDir.y = 0f;
            flatDir.Normalize();

            Vector3 target = transform.position + flatDir * dashDistance;
            target.y = transform.position.y;

            float dashTime = 0f;
            Vector3 start = transform.position;
            Vector3 end = target;

            while (dashTime < 1f)
            {
                transform.position = Vector3.Lerp(start, end, dashTime);
                dashTime += Time.deltaTime * data.moveSpeed;
                yield return null;
            }

            transform.position = end;
            yield return new WaitForSeconds(dashDelay);
        }

        isBusy = false;
    }


    private IEnumerator DoSpinShoot()
    {
        isBusy = true;
        state = State.SpinShoot;

        float timer = 0f;
        float spinAnglePerFrame = spinSpeed;
        float shootTimer = 0f;

        while (timer < spinDuration)
        {
            transform.Rotate(Vector3.up, spinAnglePerFrame * Time.deltaTime);

            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                shootTimer = 0f;

                for (int i = 0; i < bulletsPerWave; i++)
                {
                    float angle = i * (360f / bulletsPerWave);
                    Vector3 baseForward = transform.forward;
                    Quaternion spinOffset = Quaternion.Euler(0, angle, 0);
                    Vector3 dir = spinOffset * baseForward;

                    GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                    MiniBullet mini = bullet.GetComponent<MiniBullet>();
                    if (mini != null)
                    {
                        mini.Initialize(dir, 10f, data.damage);
                    }
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isBusy = false;
    }

    private IEnumerator DoHop()
    {
        isBusy = true;
        state = State.Hop;

        int hopCount = 3;
        float duration = 1f;

        for (int i = 0; i < hopCount; i++)
        {
            Vector3 startPos = transform.position;
            Vector3 targetXZ = player.position;
            targetXZ.y = startPos.y;

            float hopTime = 0f;

            while (hopTime < duration)
            {
                float t = hopTime / duration;
                float height = Mathf.Sin(t * Mathf.PI) * hopHeight;
                Vector3 flatLerp = Vector3.Lerp(startPos, targetXZ, t);
                transform.position = new Vector3(flatLerp.x, startPos.y + height, flatLerp.z);

                hopTime += Time.deltaTime * data.moveSpeed;
                yield return null;
            }

            transform.position = targetXZ;
            yield return new WaitForSeconds(dashDelay); // reuse dashDelay as rest time
        }

        isBusy = false;
    }



    private IEnumerator DoRoam()
    {
        isBusy = true;
        state = State.Roam;

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
}

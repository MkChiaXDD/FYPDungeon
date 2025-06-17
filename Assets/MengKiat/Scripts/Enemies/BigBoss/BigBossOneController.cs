using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigBossOneController : Enemy
{
    private Transform player;

    private enum State { Idle, Dash, SpinShoot, Roam }
    private State state = State.Idle;

    [Header("Boss Timing")]
    public float idleTime = 0.5f;

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

    [Header("Roam Settings")]
    public float roamDuration = 3f;

    [SerializeField] private ScreenShake screenShake;

    private bool isBusy;
    private int attackCounter;

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
                state = (State)Random.Range(1, 3); // 1=Dash, 2=SpinShoot
                attackCounter++;

                switch (state)
                {
                    case State.Dash:
                        yield return StartCoroutine(DoDash());
                        break;
                    case State.SpinShoot:
                        yield return StartCoroutine(DoSpinShoot());
                        break;
                }
            }

            state = State.Idle;
            yield return new WaitForSeconds(idleTime);
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
}

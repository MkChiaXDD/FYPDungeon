using UnityEngine;
using System.Collections;

public class BomberEnemyController : Enemy
{
    enum State { Roam, Chase, Attack }

    [Header("Ranges & Forces")]
    [SerializeField] float roamRadius = 5f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] float explosionForce = 500f;
    [SerializeField] float explosionGrowDuration = 1f;
    [SerializeField] float explodeGrowScale = 2f;
    [SerializeField] float explosionUpwardModifier = 1f;
    [SerializeField] private GameObject circleIndicator;

    [Header("Difficulty Scaling")]
    [SerializeField] int roundForScaling = 1;
    [SerializeField] float explodingSizeMultiplier = 1.5f;

    [Header("Roaming")]
    [SerializeField] float roamDelay = 3f;

    [Header("Avoidance")]
    [SerializeField] float feelerLength = 2f;
    [SerializeField] float feelerRadius = 0.2f;
    [SerializeField] float avoidWeight = 5f;
    [SerializeField] LayerMask obstacleMask;

    [Header("Movement Smoothing")]
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float smoothing = 5f;

    [Header("Visual & Effects")]
    [SerializeField] ParticleSystem explodingParticle;
    [SerializeField] GameObject model;
    [SerializeField] Light theLight;

    private float currentExplosionRadius;
    private Vector3 spawnPosition;
    private Vector3 roamTarget;
    private float roamTimer;
    private Transform player;
    private Vector3 currentDir;

    private State state;
    private bool boutaDie = false;
    private bool isExploding = false;
    public bool isPickedup = false;

    protected override void Awake()
    {
        base.Awake();
        spawnPosition = transform.position;
        player = GameObject.FindWithTag("Player").transform;
        state = State.Roam;
        ChooseRoamTarget();
        currentDir = transform.forward;

        currentExplosionRadius = currentRound < roundForScaling
            ? explosionRadius
            : explosionRadius * explodingSizeMultiplier;

        if (explodingParticle != null)
            explodingParticle.Stop();

        if (circleIndicator != null)
        {
            circleIndicator.transform.SetParent(transform);
            circleIndicator.transform.localPosition = new Vector3(0, -0.53f, 0);
            circleIndicator.transform.localScale = transform.localScale * (currentExplosionRadius * 2f);
            circleIndicator.transform.localScale = new Vector3(circleIndicator.transform.localScale.x, 0.05f, circleIndicator.transform.localScale.z);
            circleIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || isStunned || boutaDie || isPickedup)
            return;

        float distToPlayerXZ = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(player.position.x, 0, player.position.z)
        );

        if (state != State.Attack)
        {
            if (distToPlayerXZ <= data.attackRange - 1)
                state = State.Attack;
            else if (distToPlayerXZ <= data.detectionRange)
                state = State.Chase;
            else
                state = State.Roam;
        }

        switch (state)
        {
            case State.Roam:
                HandleRoam();
                break;

            case State.Chase:
                MoveWithAvoidance(player.position);
                break;

            case State.Attack:
                if (!isExploding)
                    StartCoroutine(ExplosionSequence());
                break;
        }
    }

    void HandleRoam()
    {
        roamTimer += Time.deltaTime;
        MoveWithAvoidance(roamTarget);

        if (Vector3.Distance(transform.position, roamTarget) < 0.2f || roamTimer >= roamDelay)
        {
            roamTimer = 0f;
            ChooseRoamTarget();
        }
    }

    void MoveWithAvoidance(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0;
        Vector3 seekDir = toTarget.normalized;

        Vector3 avoidDir = Vector3.zero;
        Vector3[] feelers = {
            transform.forward,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized
        };

        foreach (var f in feelers)
        {
            Vector3 dir = f;
            dir.y = 0;
            dir.Normalize();

            if (Physics.SphereCast(transform.position, feelerRadius, dir, out RaycastHit hit, feelerLength, obstacleMask))
            {
                Vector3 normal = hit.normal;
                normal.y = 0;
                float strength = (feelerLength - hit.distance) / feelerLength;
                avoidDir += normal.normalized * strength;
            }
        }

        Vector3 desired = seekDir + avoidDir * avoidWeight;
        desired.y = 0;

        if (desired == Vector3.zero)
            desired = transform.forward;

        desired.Normalize();
        currentDir = Vector3.Slerp(currentDir, desired, smoothing * Time.deltaTime);

        transform.position += currentDir * CurrentMoveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentDir), Time.deltaTime * turnSpeed);
    }

    public IEnumerator ExplosionSequence()
    {
        if (isExploding || boutaDie) yield break;

        // Slow down during charge-up
        float originalSpeed = CurrentMoveSpeed;
        SetSpeed(originalSpeed * 0.3f);

        if (circleIndicator != null)
            circleIndicator.SetActive(true);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("BomberChargeUp", this.gameObject);
        }

        boutaDie = true;
        isExploding = true;

        float t = 0f;
        float flashSpeed = 5f;
        Vector3 initialScale = model.transform.localScale;
        Vector3 targetScale = initialScale * explodeGrowScale;

        while (t < explosionGrowDuration)
        {
            model.transform.localScale = Vector3.Lerp(initialScale, targetScale, t / explosionGrowDuration);

            float lerp = Mathf.PingPong(Time.time * flashSpeed, 1f);
            theLight.color = Color.Lerp(Color.red, Color.white, lerp);

            t += Time.deltaTime;
            yield return null;
        }

        SetSpeed(originalSpeed); // Restore speed (though explosion happens immediately after)
        model.transform.localScale = targetScale;
        theLight.color = Color.red;
        Explode();
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, currentExplosionRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                if (hit.TryGetComponent<IDamageable>(out var dmg) && hit.attachedRigidbody != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    Vector3 knockbackForce = direction * explosionForce;

                    hit.attachedRigidbody.velocity = Vector3.zero;
                    hit.attachedRigidbody.AddForce(knockbackForce * 3, ForceMode.Force);
                    dmg.TakeDamage(data.damage);
                    ExplosionScreenShake();
                }
            }
            else
            {
                if (hit.gameObject.GetComponent<IDamageable>() == null)
                {
                    continue;
                }
                if (hit.attachedRigidbody != null)
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    Vector3 knockbackForce = dir * (explosionForce / 3f);
                    hit.attachedRigidbody.velocity = Vector3.zero;
                    hit.attachedRigidbody.AddForce(knockbackForce * 3, ForceMode.Force);
                }

                if (hit.TryGetComponent<BomberEnemyController>(out var bomber) && !bomber.boutaDie)
                {
                    bomber.StartCoroutine(bomber.ExplosionSequence());
                }
            }
        }

        PlayExplosionVFX();
    }

    void PlayExplosionVFX()
    {
        model.SetActive(false);

        if (explodingParticle != null)
            explodingParticle.Play();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX("BomberExplode", this.gameObject);
        }

        float duration = explodingParticle.main.duration;
        Destroy(gameObject, duration);
    }

    void ExplosionScreenShake()
    {
        StaticScreenShake.Shake(Camera.main, strongerShake);
    }

    void ChooseRoamTarget()
    {
        Vector2 rnd = Random.insideUnitCircle * roamRadius;
        roamTarget = new Vector3(
            spawnPosition.x + rnd.x,
            transform.position.y,
            spawnPosition.z + rnd.y
        );
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, currentExplosionRadius);
        }
#endif
    }
}
using System.Collections;
using UnityEngine;

public class FatBoss : MonoBehaviour
{
    [Header("Combat Parameters")]
    [SerializeField] private float chargeSpeed = 20f;
    [SerializeField] private float chargeWindup = 1.5f;
    [SerializeField] private float slamHeight = 8f;
    [SerializeField] private float slamDamageRadius = 5f;
    [SerializeField] private int slamDamage = 30;
    [SerializeField] private float timeBetweenAttacks = 3f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem chargeParticles;
    [SerializeField] private ParticleSystem slamImpactParticles;

    private Transform player;
    private Rigidbody rb;
    private Animator anim;
    private Vector3 chargeDirection;
    private float attackCooldown;
    private bool isAttacking;

    private enum AttackState { None, ChargeWindup, Charging, SlamWindup, Slamming }
    private AttackState currentState = AttackState.None;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        attackCooldown = timeBetweenAttacks;
    }

    private void Update()
    {
        if (isAttacking) return;

        attackCooldown -= Time.deltaTime;
        if (attackCooldown <= 0)
        {
            ChooseAttack();
            attackCooldown = timeBetweenAttacks;
        }
    }

    private void ChooseAttack()
    {
        isAttacking = true;
        currentState = (Random.value > 0.5f) ? AttackState.ChargeWindup : AttackState.SlamWindup;

        switch (currentState)
        {
            case AttackState.ChargeWindup:
                StartCoroutine(ChargeAttack());
                break;
            case AttackState.SlamWindup:
                StartCoroutine(SlamAttack());
                break;
        }
    }

    private IEnumerator ChargeAttack()
    {
        // Windup phase
        chargeDirection = (player.position - transform.position).normalized;
        chargeDirection.y = 0;
       // anim.SetTrigger("ChargeWindup");
        if (chargeParticles) chargeParticles.Play();

        yield return new WaitForSeconds(chargeWindup);

        // Charge execution
        currentState = AttackState.Charging;
       // anim.SetTrigger("Charge");
        rb.AddForce(chargeDirection * chargeSpeed, ForceMode.VelocityChange);

        // Auto-stop after 2 seconds
        yield return new WaitForSeconds(2f);
        ResetAfterAttack();
    }

    private IEnumerator SlamAttack()
    {
        // Windup phase
        //anim.SetTrigger("SlamWindup");
        yield return new WaitForSeconds(1f);

        // Jump phase
        currentState = AttackState.Slamming;
        rb.AddForce(Vector3.up * slamHeight, ForceMode.Impulse);
        //anim.SetTrigger("Jump");

        // Wait for apex of jump
        yield return new WaitUntil(() => rb.velocity.y < 0);

        // Slam down
       // anim.SetTrigger("Slam");
        rb.AddForce(2 * slamHeight * Vector3.down, ForceMode.Impulse);

        // Detect landing
        yield return new WaitUntil(() => Physics.Raycast(transform.position, Vector3.down, 1f));
        ApplySlamDamage();
        if (slamImpactParticles) slamImpactParticles.Play();

        yield return new WaitForSeconds(0.5f);
        ResetAfterAttack();
    }

    private void ApplySlamDamage()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, slamDamageRadius, playerLayer);
        foreach (Collider playerCollider in hitPlayers)
        {
            playerCollider.GetComponent<IDamageable>().TakeDamage(slamDamage);
        }
    }

    private void ResetAfterAttack()
    {
        rb.velocity = Vector3.zero;
        isAttacking = false;
        currentState = AttackState.None;
        if (chargeParticles) chargeParticles.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Charge impact damage
        if (currentState == AttackState.Charging && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(20);
            ResetAfterAttack();
        }

        // Stop charge when hitting walls
        if (currentState == AttackState.Charging && collision.gameObject.CompareTag("Wall"))
        {
            ResetAfterAttack();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Show slam radius in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamDamageRadius);
    }
}
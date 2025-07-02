using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

public class Boom : Projectile , IDamageable
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private SpellHitbox Hitbox;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackMultiplier = 10f;
    [SerializeField] private int HitPoint = 10;
    [SerializeField] private bool _isExploding = true;
    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Debug.Log("Ignite: " + duration);
        StartCoroutine(FuseCountdown(duration));
        _rb.AddForce(PlayerMovement.Instance.GetDirection() * 20, ForceMode.Impulse);
    }

    private IEnumerator FuseCountdown(float Duration)
    {
        Debug.Log("Ignite: " + Duration);
        yield return new WaitForSeconds(Duration);
        yield return StartCoroutine(Exploding());
    }

    public void Die()
    {
        if (!_isExploding)
        {
            StartCoroutine(Exploding());
        }
    }

    public void TakeDamage(float damage)
    {
       // Debug.Log("Take KnockBack");
        HitPoint--;
        _rb.AddForce(PlayerMovement.Instance.GetDirection() * knockbackMultiplier * damage, ForceMode.Impulse);
        if (HitPoint <= 0)
        {
            Die();
        }
    }

    public void TakeElementalDamage(float damage, ElementType element)
    {
        HitPoint--;

        _rb.AddForce(PlayerMovement.Instance.GetDirection() * knockbackMultiplier * damage, ForceMode.Impulse);
        if (HitPoint <= 0)
        {
            Die();
        }

        if (element == ElementType.Pyro)
        {
            StartCoroutine(FuseCountdown(1f));
        }
    }

    private IEnumerator Exploding()
    {
        if (_isExploding == true) {
            _isExploding = false;
            Debug.Log("Explode");
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, Radius);
            foreach (var hit in hitColliders)
            {
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                }

                if (hit.TryGetComponent<Rigidbody>(out var rb))
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                }
            }

            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
            }

            yield return null;
            Destroy(gameObject);
        }


    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

}

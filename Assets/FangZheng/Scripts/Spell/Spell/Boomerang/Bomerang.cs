using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomerang : Projectile
{
    [SerializeField] private Vector3 StartPos;
    [SerializeField] private bool IsReturning;
    [SerializeField] private Vector3 direction;
    [SerializeField] SpellHitbox Hitbox;
    //[SerializeField] PlayerMovement PlayerMovement;

    public void Start()
    {
        if (Hitbox == null)
        {
            Hitbox = this.GetComponent<SpellHitbox>();
        }
        Hitbox.Initit(spellCast);

        direction = new Vector3(PlayerMovement.Instance.GetDirection().x, 0, PlayerMovement.Instance.GetDirection().z);
        StartPos = this.transform.position;
        Destroy(this.gameObject, duration);
        StartCoroutine(BoomerangMove());
    }

    public IEnumerator BoomerangMove()
    {
        float distanceTraveled = 0f;
        Vector3 direction = transform.forward;

        while (distanceTraveled < Range && !IsReturning)
        {
            transform.position += direction * Speed * Time.deltaTime;
            distanceTraveled = Vector3.Distance(StartPos, transform.position);

            transform.Rotate(Vector3.up, 720 * Time.deltaTime);

            yield return null;
        }

        IsReturning = true;

        yield return null;

        while (Vector3.Distance(transform.position, StartPos) > 0.1f)
        {
            direction = (StartPos - transform.position).normalized;
            transform.position += direction * Speed * Time.deltaTime;

            transform.Rotate(Vector3.up, 720 * Time.deltaTime);

            yield return null;
        }

        Destroy(gameObject);
    }
}

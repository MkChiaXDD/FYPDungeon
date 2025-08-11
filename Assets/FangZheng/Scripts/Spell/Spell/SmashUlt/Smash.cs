using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Smash : Projectile
{
    [SerializeField] private GameObject Hitbox;
    [SerializeField] private GameObject HitboxPrefab;
    [SerializeField] private Tornado Tornado;
    [SerializeField] private SpellCast SpellCast;
    [SerializeField] private GameObject Player;
    [SerializeField] private float SmashCountDown;
    [SerializeField] private float SmashDelay;

    private bool hasSmashed = false;
    private bool TorandoActivate = false;
    private GameObject Tornado_created;
    private void Start()
    {
        //if (Hitbox == null)
        //{
        //    Hitbox = this.GetComponent<SpellHitbox>();
        //}
        
        //if (Hitbox != null)
        //{
        //    Hitbox.enabled = false;
        //}
        Player = FindFirstObjectByType<PlayerMovement>().gameObject;
        SummonTornado();
        StartCoroutine(PrepSmash());
    }

    private void SummonTornado()
    {
        Tornado_created = Instantiate(Tornado.gameObject, new Vector3(Player.transform.position.x, 0, Player.transform.position.z), Quaternion.identity);
        Tornado_created.GetComponent<Tornado>().PullRadius = Radius;
        Tornado_created.transform.localScale = new Vector3(Radius * 10 , Radius / 2 , Radius * 10);
        TorandoActivate = true;
        hasSmashed = false;
    }

    private IEnumerator PrepSmash()
    {

        yield return new WaitForSeconds(duration - SmashDelay);
        hasSmashed = true;
        Player.GetComponent<PlayerMovement>().StunPlayer(SmashDelay);

        PerformSmash();

        yield return new WaitForSeconds(SmashDelay);


        if (Hitbox != null)
        {
            Destroy(Hitbox.gameObject);
        }

        if (Tornado_created != null)
        {
            Destroy(Tornado_created.gameObject);
        }

        Destroy(gameObject);

        //TorandoActivate = false;
    }

    private void PerformSmash()
    {
        TorandoActivate = true;

        

        if (HitboxPrefab != null)
        {
            Hitbox = Instantiate( HitboxPrefab, new Vector3(Player.transform.position.x, 0, Player.transform.position.z), Quaternion.identity);
            Hitbox.transform.localScale = Vector3.one * Range ;
            Hitbox.GetComponent<SpellHitbox>().Initit(spellCast);

        }
    }

    private void Update()
    {
        if (!hasSmashed && TorandoActivate)
        {
            Tornado_created.transform.position = new Vector3(Player.transform.position.x, 0, Player.transform.position.z);
        }
    }
}

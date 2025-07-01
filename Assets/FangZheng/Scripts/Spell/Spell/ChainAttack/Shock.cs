using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Shock : Projectile
{
    [SerializeField] SpellHitbox Hitbox;

    private List<GameObject> hitEnemies = new List<GameObject>();
    public void Start()
    {
        if (Hitbox == null)
        {
            Hitbox = this.GetComponent<SpellHitbox>();
        }
        Hitbox.Initit(spellCast);

        Destroy(this.gameObject, duration);

    }

    public void Update()
    {

    }

    protected void OnTriggerEnter(Collider other)
    {
        if (hitEnemies.Contains(other.gameObject)) return;


    }

    private void Chain()
    {
        //Collider[] enemies = Physics.OverlapSphere(transform.position, radius, spellCast.hitLayers);
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlashUlt : Projectile
{
    [SerializeField] private SpellHitbox Hitbox;
    [SerializeField] private ParticleSystem ParticleSystem;


    private void Start()
    {
        if (Hitbox == null)
        {
            Hitbox = this.GetComponent<SpellHitbox>();
        }
        if (ParticleSystem == null)
        {
            ParticleSystem = this.GetComponent<ParticleSystem>();
        }
        Hitbox.Initit(spellCast);
        //Modify();
    }

    public void Modify()
    {
        if (this.GetComponent<SphereCollider>() == null)
        {
            this.AddComponent<SphereCollider>();
        }
        this.transform.localScale = Vector3.one * Radius; 
        //this.GetComponent<SphereCollider>().radius = Radius;

        //if (ParticleSystem != null)
        //{
        //    ParticleSystem.gameObject.transform.localScale = Vector3.one * Radius;
        //}

        //transform.localScale = Vector3.one * Radius * 2;
        Destroy(this.gameObject, duration);
    }
}

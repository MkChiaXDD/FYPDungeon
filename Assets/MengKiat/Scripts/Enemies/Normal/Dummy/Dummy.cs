using System;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : Enemy
{
    private enum Type
    {
        Nothing,
        Attack,
        Shield
    }
    [SerializeField] private Type DummyType ;
    [SerializeField] private float attackCooldown;

    private Transform player;
    private float attackTimer;
    private Vector3 currentDir;
    //1 = have shield 2 = have shield before but broke 0 is no shield at all
    private int HaveShield = 0;
    private bool Shieldbreak;
    [SerializeField] private GameObject EnemyShield;
    private TutorialProggresion tutorial;
    public event Action<string> OnAction;
    

    protected override void Awake()
    {
        base.Awake();
        tutorial = FindFirstObjectByType<TutorialProggresion>();
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        currentDir = transform.forward;

        switch (DummyType)
        {
            case Type.Shield:
                EnemyShield = GetComponentInChildren<EnemyShield>()?.gameObject;

                if (EnemyShield == null)
                {
                    Debug.LogWarning("Shield dummy type selected but no shield found in children");
                }
                else
                {
                    HaveShield = 1;
                }
                break;
        }

    }

    private void Update()
    {
        if (isStunned )
        {
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f && DummyType == Type.Attack)
        {
            Attack();
            attackTimer = attackCooldown;
        }

        if (EnemyShield != null) {
            if (EnemyShield.active == false)
            {
                EnemyShield = null;
            }

            if (EnemyShield == null && HaveShield == 1)
            {
                HaveShield = 2;
                tutorial.IfPlayerPerformAction("BreakEnemy");
                OnAction?.Invoke("BreakEnemy");
            }
        }

        //if (EnemyShield.active == false)
        //{

        //}

    }


    private void Attack()
    {
        float distance = Vector3.Distance(new Vector3(this.transform.position.x , 0 , this.transform.position.z) , new Vector3(player.transform.position.x , 0 , player.transform.position.z));

        if (distance <= data.attackRange)
        {
            IDamageable damageable = player.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                tutorial.IfPlayerPerformAction("AttackEnemy");
                OnAction?.Invoke("AttackEnemy");
            }
        }
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        tutorial.IfPlayerPerformAction("AttackEnemy");
        OnAction?.Invoke("AttackEnemy");
    }

    public override void Die()
    {
        base.Die();
        tutorial.IfPlayerPerformAction("DeadEnemy");
        OnAction?.Invoke("DeadEnemy");
    }
}

using UnityEngine;

public class Enemy : DamageableEntity
{
    [SerializeField] protected EnemyData data;

    private void Start()
    {
        SetMaxHealth(data.maxHealth);
    }
}

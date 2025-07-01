using System.Collections;
using UnityEngine;

public interface IDamageable
{
   public void TakeDamage(float damage);
    public void TakeElementalDamage(float damage, ElementType element);
   public void Die();

}




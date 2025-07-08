using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    private float maxHp;
    private float currentHp;

    public void Init(float maxHp)
    {
        this.maxHp = maxHp;
        currentHp = this.maxHp;
    }

    public void HitShield(float damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public float GetShieldHp()
    {
        return currentHp;
    }
}

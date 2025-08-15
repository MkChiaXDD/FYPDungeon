using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float maxHealth;
    public float moveSpeed;
    public float damage;
    public float detectionRange;
    public float attackRange;

    [Header("Resistances")] 
    [Tooltip("1 = Normal, >1 = Resistant, <1 = Weak")]
    [Range(0, 2)] public float pyroResistance = 1f;
    [Range(0, 2)] public float hydroResistance = 1f;
    [Range(0, 2)] public float electroResistance = 1f;
    [Range(0, 2)] public float cryoResistance = 1f;

    public enum EnemyType
    {
        normalEnemy,
        notNormalEnemy
    }

    [Tooltip("Type of enemy behavior pattern")]
    public EnemyType enemyType;
}

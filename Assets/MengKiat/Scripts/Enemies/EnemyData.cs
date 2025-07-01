using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHealth;
    public float moveSpeed;
    public int damage;

    public enum EnemyType
    {
        normalEnemy,
        notNormalEnemy
    }

    [Tooltip("Type of enemy behavior pattern")]
    public EnemyType enemyType; // ? this will show up in the Inspector as a dropdown
}

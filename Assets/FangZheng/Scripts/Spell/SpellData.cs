using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Spell Data")]
public class SpellData : ScriptableObject
{
    public string weaponName;
    public int damage;
    public LayerMask hitLayers;
}

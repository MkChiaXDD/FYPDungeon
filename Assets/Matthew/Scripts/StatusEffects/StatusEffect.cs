using UnityEngine;
public abstract class StatusEffect : ScriptableObject
{
    public string effectName;
    public Sprite icon;
    public float duration;
    public bool isStackable;
    public int maxStacks = 1;

    public abstract void ApplyEffect(StatusEffectReceiver receiver);
    public abstract void RemoveEffect(StatusEffectReceiver receiver);
    public abstract void UpdateEffect(StatusEffectReceiver receiver);
}
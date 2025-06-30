using System.Collections.Generic;
using UnityEngine;

public class StatusEffectReceiver : MonoBehaviour
{
    private Dictionary<StatusEffect, ActiveEffect> activeEffects = new Dictionary<StatusEffect, ActiveEffect>();

    public PoisonEffect poison;
    public StunEffect stun;
    public SpeedBoostEffect speedBoost;

    void Update()
    {
        // Create a copy to avoid modification during iteration
        var effectsCopy = new List<ActiveEffect>(activeEffects.Values);

        foreach (var effect in effectsCopy)
        {
            if (activeEffects.ContainsKey(effect.effect))
            {
                effect.Update(Time.deltaTime, this);
            }
        }

      

   
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    GetComponent<StatusEffectReceiver>().ApplyEffect(poison);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    GetComponent<StatusEffectReceiver>().ApplyEffect(stun);
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    GetComponent<StatusEffectReceiver>().ApplyEffect(speedBoost);
        //}
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    GetComponent<StatusEffectReceiver>().DebugActiveEffects();
        //}
    
}

    public void ApplyEffect(StatusEffect effect)
    {
        Debug.Log($"[Status System] Applying {effect.effectName} to {gameObject.name}");

        if (activeEffects.TryGetValue(effect, out ActiveEffect activeEffect))
        {
            if (effect.isStackable && activeEffect.stacks < effect.maxStacks)
            {
                Debug.Log($"[Status System] Adding stack to {effect.effectName} on {gameObject.name}. New stacks: {activeEffect.stacks + 1}");
                activeEffect.AddStack();
            }
            Debug.Log($"[Status System] Refreshing duration of {effect.effectName} on {gameObject.name}");
            activeEffect.ResetDuration();
        }
        else
        {
            Debug.Log($"[Status System] Creating new instance of {effect.effectName} on {gameObject.name}");
            var newEffect = new ActiveEffect(effect);
            activeEffects.Add(effect, newEffect);
            newEffect.Apply(this);
        }
    }

    public void RemoveEffect(StatusEffect effect)
    {
        if (activeEffects.TryGetValue(effect, out ActiveEffect activeEffect))
        {
            Debug.Log($"[Status System] Removing {effect.effectName} from {gameObject.name}");
            activeEffect.Remove(this);
            activeEffects.Remove(effect);
        }
        else
        {
            Debug.LogWarning($"[Status System] Tried to remove non-existent effect {effect.effectName} from {gameObject.name}");
        }
    }

    public void DebugActiveEffects()
    {
        Debug.Log($"[Status System] === Active Effects on {gameObject.name} ===");
        foreach (var kvp in activeEffects)
        {
            Debug.Log($"{kvp.Key.effectName} - {kvp.Value.remainingTime:0.00}s remaining, Stacks: {kvp.Value.stacks}");
        }
    }

    private class ActiveEffect
    {
        public StatusEffect effect;
        public float remainingTime;
        public int stacks = 1;

        public ActiveEffect(StatusEffect effect)
        {
            this.effect = effect;
            remainingTime = effect.duration;
            Debug.Log($"[Status System] Created {effect.effectName} instance. Duration: {effect.duration}s");
        }

        public void Apply(StatusEffectReceiver receiver)
        {
            Debug.Log($"[Status System] Applying initial effect: {effect.effectName} to {receiver.gameObject.name}");
            effect.ApplyEffect(receiver);
        }

        public void Remove(StatusEffectReceiver receiver)
        {
            Debug.Log($"[Status System] Removing effect: {effect.effectName} from {receiver.gameObject.name}");
            effect.RemoveEffect(receiver);
        }

        public void Update(float deltaTime, StatusEffectReceiver receiver)
        {
            remainingTime -= deltaTime;

            if (remainingTime <= 0)
            {
                Debug.Log($"[Status System] {effect.effectName} expired on {receiver.gameObject.name}");
                Remove(receiver);
                receiver.activeEffects.Remove(effect);
            }
            else
            {
                effect.UpdateEffect(receiver);
            }
        }

        public void ResetDuration()
        {
            Debug.Log($"[Status System] Resetting {effect.effectName} duration from {remainingTime:0.00}s to {effect.duration}s");
            remainingTime = effect.duration;
        }

        public void AddStack()
        {
            stacks = Mathf.Min(stacks + 1, effect.maxStacks);
            Debug.Log($"[Status System] Added stack to {effect.effectName}. Now at {stacks}/{effect.maxStacks} stacks");
        }
    }
}

// Example Status Effects with debug logging
[CreateAssetMenu(menuName = "Status Effects/Poison")]
public class PoisonEffect : StatusEffect
{
    public float damagePerSecond = 5f;
    private float damageTimer;
    private Color originalColour;
    public Color damageEffectColour = Color.green;

 
    public override void ApplyEffect(StatusEffectReceiver receiver)
    {
        if (receiver.TryGetComponent<Renderer>(out var originalRenderer))
        {
            if (originalRenderer.material.color != Color.green)
            {
                originalColour = originalRenderer.material.color;
            }
            else
                originalColour = Color.green; //this check is for when the thing is green
        }

        Debug.Log($"[Poison] Applying poison effect to {receiver.gameObject.name}");
        if (receiver.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = damageEffectColour;
        }
    }

    public override void RemoveEffect(StatusEffectReceiver receiver)
    {
        Debug.Log($"[Poison] Removing poison effect from {receiver.gameObject.name}");
        if (receiver.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material.color = originalColour;
        }
    }

    public override void UpdateEffect(StatusEffectReceiver receiver)
    {
        damageTimer += Time.deltaTime;
        if (damageTimer >= 1f)
        {
            Debug.Log($"[Poison] Applying damage to {receiver.gameObject.name}");
            if (receiver.TryGetComponent<IDamageable>(out var health))
            {
                health.TakeDamage(damagePerSecond);
            }
            damageTimer = 0;
        }
    }
}

[CreateAssetMenu(menuName = "Status Effects/Fire")]
public class FireEffect : StatusEffect
{
    
    public override void ApplyEffect(StatusEffectReceiver receiver)
    {
        Debug.Log($"[Fire] Applying Fire to {receiver.gameObject.name}");
        if (receiver.TryGetComponent<PlayerController>(out var playerMovement))
        {
            //playerMovement.SetMovementEnabled(false);
            Debug.Log($"[Fire] Applied Fire to {receiver.gameObject.name}");
            return;
        }
        if (receiver.TryGetComponent<Enemy>(out var enemyAI))
        {
            //enemyAI.SetAIEnabled(false);
            Debug.Log($"[Fire] Applied Fire to {receiver.gameObject.name}");
            return;
        }

        Debug.Log($"[Fire] No targets to Fire!");
    }

    public override void RemoveEffect(StatusEffectReceiver receiver)
    {
        Debug.Log($"[Fire] Removing Fire from {receiver.gameObject.name}");
        if (receiver.TryGetComponent<PlayerController>(out var playerMovement))
        {
            //playerMovement.SetMovementEnabled(true);
            Debug.Log($"[Fire] Removed Fire from {receiver.gameObject.name}");
            return;

        }
        if (receiver.TryGetComponent<Enemy>(out var enemyAI))
        {
            // enemyAI.SetAIEnabled(true);
            Debug.Log($"[Fire] Removed Fire from {receiver.gameObject.name}");
            return;
        }

    }

    public override void UpdateEffect(StatusEffectReceiver receiver)
    {

        Debug.Log($"[Fire] {receiver.gameObject.name} is Fired ({duration:0.00}s remaining)");
    }
}

[CreateAssetMenu(menuName = "Status Effects/Stun")]
public class StunEffect : StatusEffect
{
    public override void ApplyEffect(StatusEffectReceiver receiver)
    {
        Debug.Log($"[Stun] Applying stun to {receiver.gameObject.name}");
        if (receiver.TryGetComponent<PlayerController>(out var playerMovement))
        {
            //playerMovement.SetMovementEnabled(false);
            Debug.Log($"[Stun] Applied stun to {receiver.gameObject.name}");
            return;
        }
        if (receiver.TryGetComponent<Enemy>(out var enemyAI))
        {
            //enemyAI.SetAIEnabled(false);
            Debug.Log($"[Stun] Applied stun to {receiver.gameObject.name}");
            return;
        }

        Debug.Log($"[Stun] No targets to stun!");
    }

    public override void RemoveEffect(StatusEffectReceiver receiver)
    {
        Debug.Log($"[Stun] Removing stun from {receiver.gameObject.name}");
        if (receiver.TryGetComponent<PlayerController>(out var playerMovement))
        {
            //playerMovement.SetMovementEnabled(true);
            Debug.Log($"[Stun] Removed stun from {receiver.gameObject.name}");
            return ;

        }
        if (receiver.TryGetComponent<Enemy>(out var enemyAI))
        {
           // enemyAI.SetAIEnabled(true);
            Debug.Log($"[Stun] Removed stun from {receiver.gameObject.name}");
            return;
        }
        
    }

    public override void UpdateEffect(StatusEffectReceiver receiver)
    {
        
        Debug.Log($"[Stun] {receiver.gameObject.name} is stunned ({duration:0.00}s remaining)");
    }
}

[CreateAssetMenu(menuName = "Status Effects/Speed Boost")]
public class SpeedBoostEffect : StatusEffect
{
    public float speedMultiplier = 1.5f;
    private Dictionary<StatusEffectReceiver, float> originalSpeeds = new Dictionary<StatusEffectReceiver, float>();

    public override void ApplyEffect(StatusEffectReceiver receiver)
    {
        Debug.Log($"[Speed Boost] Applying to {receiver.gameObject.name}");
        if (receiver.TryGetComponent<PlayerController>(out var movement))
        {
            //originalSpeeds[receiver] = movement.Speed;
           // movement.Speed *= speedMultiplier;
            //Debug.Log($"[Speed Boost] Speed increased from {originalSpeeds[receiver]} to {movement.Speed}");
        }
    }

    public override void RemoveEffect(StatusEffectReceiver receiver)
    {
        Debug.Log($"[Speed Boost] Removing from {receiver.gameObject.name}");
        if (originalSpeeds.TryGetValue(receiver, out float originalSpeed))
        {
            if (receiver.TryGetComponent<CharacterController>(out var movement))
            {
                //movement.Speed = originalSpeed;
                Debug.Log($"[Speed Boost] Speed restored to {originalSpeed}");
            }
            originalSpeeds.Remove(receiver);
        }
    }

    public override void UpdateEffect(StatusEffectReceiver receiver)
    {
        Debug.Log($"[Speed Boost] Active on {receiver.gameObject.name} ({duration:0.00}s remaining)");
    }
}

// Helper class for testing
public class DebugStatusTester : MonoBehaviour
{
    //public PoisonEffect poison;
    //public StunEffect stun;
    //public SpeedBoostEffect speedBoost;

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Alpha1))
    //    {
    //        GetComponent<StatusEffectReceiver>().ApplyEffect(poison);
    //    }
    //    if (Input.GetKeyDown(KeyCode.Alpha2))
    //    {
    //        GetComponent<StatusEffectReceiver>().ApplyEffect(stun);
    //    }
    //    if (Input.GetKeyDown(KeyCode.))
    //    {
    //        GetComponent<StatusEffectReceiver>().ApplyEffect(speedBoost);
    //    }
    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        GetComponent<StatusEffectReceiver>().DebugActiveEffects();
    //    }
    //}
}
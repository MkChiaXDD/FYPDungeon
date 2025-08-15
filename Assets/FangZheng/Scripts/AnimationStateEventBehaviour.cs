using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class AnimationStateEventBehaviour : StateMachineBehaviour
{
    public string eventName;
    [Range(0f, 1f)] public float TriggerTime;

    bool _hasTrigger;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _hasTrigger = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float currentTime = stateInfo.normalizedTime % 1f;

        if (!_hasTrigger && currentTime >= TriggerTime)
        {
            NotificationReciever(animator);
            _hasTrigger = true;
        }
    }

    void NotificationReciever(Animator animator)
    {
        AnimationEventTracker Tracker = animator.GetComponent<AnimationEventTracker>();

        if (Tracker != null)
        {
            Tracker.OnAnimationEventTrigger(eventName);
        }
    }
}

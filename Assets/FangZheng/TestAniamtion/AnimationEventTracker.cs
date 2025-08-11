using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventTracker : MonoBehaviour
{
    [SerializeField] private List<AnimationEvent> events = new List<AnimationEvent>();

    public void OnAnimationEventTrigger(string EventName)
    {
        //AnimationEvent Corresponding_Event = events.Find();
    }
}

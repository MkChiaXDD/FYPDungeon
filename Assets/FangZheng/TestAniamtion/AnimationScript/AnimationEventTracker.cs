using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventTracker : MonoBehaviour
{
    [SerializeField] private List<animEvent> events = new List<animEvent>();
    [SerializeField] private GameObject Player;
    public void OnAnimationEventTrigger(string EventName)
    {
        //AnimationEvent Corresponding_Event = events.Find();
        animEvent matchingEvent = events.Find(x => x.eventName == EventName);
        matchingEvent?.OnAniamtionEvent?.Invoke();
    }
}

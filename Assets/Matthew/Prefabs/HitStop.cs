using System.Collections;
using UnityEngine;

//<summary>
//Class to create a short time stop effect whenever attacks land which makes the hits feel more impactful
//<summary>
public class HitStop : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float defaultDuration = 0.1f;
    [SerializeField] private float defaultTimeScale = 0.02f;
    [SerializeField]
    private AnimationCurve timeScaleCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(1, 1)
    );

    private Coroutine currentHitStop;
    private float originalTimeScale = 1;

    //<summary>
    //helper function to trigger hitstop
    //to ensure that each hitstop ends properly 
    //<summary>
    public void TriggerHitStop(float intensity = 1f)
    {
        if (currentHitStop != null)
        {
            StopCoroutine(currentHitStop);
            Time.timeScale = originalTimeScale; // Reset immediately when interrupted
        }

        currentHitStop = StartCoroutine(DoHitStop(defaultDuration * intensity));
    }


    //<summary>
    //Triggers a short time stop, keep the duration small (below 0.5s)
    //<summary>
    private IEnumerator DoHitStop(float duration)
    {
        float elapsed = 0f;
        Time.timeScale = defaultTimeScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            Time.timeScale = Mathf.Lerp(defaultTimeScale, originalTimeScale, timeScaleCurve.Evaluate(t));
            yield return null;
        }

        Time.timeScale = originalTimeScale;
        currentHitStop = null;
    }
}
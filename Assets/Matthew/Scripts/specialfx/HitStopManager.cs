using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    // Singleton instance
    private static HitStopManager _instance;
    public static HitStopManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HitStopManager>();

                // Create new instance if none exists
                if (_instance == null)
                {
                    GameObject obj = new GameObject("HitStopManager");
                    _instance = obj.AddComponent<HitStopManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Settings")]
    [SerializeField] private float defaultDuration = 0.1f;
    [SerializeField] private float targetTimeScale = 0.02f;
    [SerializeField]
    private AnimationCurve timeScaleCurve = new AnimationCurve(
        new Keyframe(0, 0),
        new Keyframe(1, 1)
    );

    private Coroutine currentHitStop;
    private float originalTimeScale = 1;

    void Awake()
    {
        // Enforce singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            // dont persist through scene
            // DontDestroyOnLoad(gameObject);
        }

        // Cache original time scale
        originalTimeScale = Time.timeScale;
    }

    // Public static access (default)
    public static void ActivateHitStopGlobal()
    {
        Instance.HitStop(Instance.defaultDuration, Instance.targetTimeScale);
    }

    // Public static access (can modify)
    public static void ActivateHitStopGlobal(float duration, float timeScale)
    {
        Instance.HitStop(duration, timeScale);
    }

    // non-static implementation
    public void HitStop(float duration, float targetTimeScale)
    {
        if (currentHitStop != null)
        {
            StopCoroutine(currentHitStop);
            Time.timeScale = originalTimeScale;
        }
        currentHitStop = StartCoroutine(DoHitStop(duration, targetTimeScale));
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            Time.timeScale = originalTimeScale;
            _instance = null;
        }
    }

    private IEnumerator DoHitStop(float duration, float targetTimeScale)
    {
        float elapsed = 0f;
        Time.timeScale = targetTimeScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            Time.timeScale = Mathf.Lerp(targetTimeScale, originalTimeScale, timeScaleCurve.Evaluate(t));
            yield return null;
        }

        Time.timeScale = originalTimeScale;
        currentHitStop = null;
    }

    public void ResetHitstop()
    {
        Time.timeScale = originalTimeScale;
    }



    
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("References")]
    [SerializeField] private Image fadeOverlay;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unsubscribe to avoid memory leaks
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene changed to: {scene.name}");
        FadeBackIn(); // Call your desired method
    }



    public void FadeToScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        // Fade out
        yield return StartCoroutine(FadeRoutine(0, 1));

        // Load new scene
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!loadOperation.isDone)
        {
            yield return null;
        }


        // Fade in
        yield return StartCoroutine(FadeRoutine(1, 0));
    }

    private IEnumerator FadeBackIn()
    {
        yield return StartCoroutine(FadeRoutine(1, 0));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = fadeOverlay.color;

        while (elapsed < fadeDuration)
        {       
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsed / fadeDuration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadeOverlay.color = color;
    }
}
using TMPro;
using UnityEngine;
using System.Collections;

public class PopUpText : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private CanvasGroup canvasGroup;
    private float duration;

    public void Initialize(string message, float duration, Color color)
    {
        this.duration = duration;

        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.color = color;
        }

        StartCoroutine(FadeAndMove());
    }

    private IEnumerator FadeAndMove()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 50f;

        float holdTime = Mathf.Max(0f, duration - 0.5f);
        float fadeTime = 0.5f;

        yield return new WaitForSeconds(holdTime);

        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            float t = elapsed / fadeTime;
            transform.position = Vector3.Lerp(startPos, endPos, t);

            if (canvasGroup != null)
                canvasGroup.alpha = 1 - t;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

}

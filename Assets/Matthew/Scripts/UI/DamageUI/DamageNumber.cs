using UnityEngine;
using TMPro;
using System.Collections;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float lifeTime = 1f;

    [SerializeField] private float scaleDuration = 0.5f; // Duration of the scale-up animation
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curve for smooth scaling





    private Vector3 floatDirection;
    private Color originalColor;
    private RectTransform rectTransform;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        rectTransform = GetComponent<RectTransform>();
        originalColor = damageText.color;

        // Randomize float direction slightly for natural look
        floatDirection = new Vector3(
            Random.Range(-0.3f, 0.3f),
            1f,
            0
        ).normalized;
    }

    public void Initialize(float damage, ElementType element)
    {
        damageText.text = damage.ToString();
        damageText.color = GetElementColor(element);
        StartCoroutine(Animate());
    }

    public void Initialize(float damage, PhysicalAttackType physicalAttackType)
    {
        damageText.text = damage.ToString();
        damageText.color = Color.red;
        StartCoroutine(Animate());
    }

    public void Initialize(float damage)
    {
        damageText.text = damage.ToString();
        damageText.color = Color.white;
        StartCoroutine(Animate());
    }

    public void Initialize(float damage, Color color)
    {
        damageText.text = damage.ToString();
        damageText.color = color;
        StartCoroutine(Animate());
    }

    //DEBUG MODE
    public void Initialize(string text, Color color)
    {
        damageText.text = text; // Now accepts any string
        damageText.color = color;
        // ... rest of initialization ...
        StartCoroutine(Animate());
    }



    private IEnumerator Animate()
    {
        float elapsed = 0f;
        Vector3 startPosition = rectTransform.position;

        // Scale up animation
        float scaleElapsed = 0f;
        while (scaleElapsed < scaleDuration)
        {
            scaleElapsed += Time.deltaTime;
            float progress = scaleCurve.Evaluate(scaleElapsed / scaleDuration);
            rectTransform.localScale = originalScale * progress;
            yield return null;
        }
        rectTransform.localScale = originalScale;


        while (elapsed < lifeTime)
        {
            // Float upward
            rectTransform.position = startPosition + floatDirection * floatSpeed * elapsed;

            // Fade out
            if (elapsed > lifeTime - fadeDuration)
            {
                float fadeProgress = (elapsed - (lifeTime - fadeDuration)) / fadeDuration;
                damageText.color = Color.Lerp(originalColor, Color.clear, fadeProgress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }



    private Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Pyro => new Color(1f, 0.4f, 0.2f), // Orange-red
            ElementType.Hydro => new Color(0.2f, 0.5f, 1f),  // Blue
            ElementType.Electro => new Color(0.7f, 0.2f, 1f), // Purple
            ElementType.Cryo => new Color(0.2f, 0.8f, 1f),   // Light blue
            _ => Color.black
        };
    }
}
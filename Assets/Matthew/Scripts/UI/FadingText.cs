using System.Collections;
using TMPro;
using UnityEngine;

public class FadingText : MonoBehaviour
{
    [Header("Text Fading Values")]
    [SerializeField] private float fadingTime = 1f;

    [Header("Bouncing Physics")]
    [SerializeField] private Vector2 initialVelocity = new Vector2(0, 300);
    [SerializeField] private float gravity = -800f;
    [SerializeField] private float bounceDamping = 0.7f;
    [SerializeField] private float horizontalDrag = 0.95f;

    [Header("Noise Maps")]
    [SerializeField] private NoiseProfile[] noiseProfiles;
    [SerializeField] private int defaultNoiseProfileIndex = 0;

    private RectTransform rectTransform;
    private Vector2 currentVelocity;
    private Vector2 currentAnchoredPosition;
    private float groundLevel;
    private NoiseProfile activeNoiseProfile;
    private TMP_Text textComponent;
    private bool isInitialized;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        InitializeBouncing();
        StartFadeOut();
    }

    private void Update()
    {
        if (isInitialized)
        {
            ApplyBouncingPhysics();
        }
    }

    /// <summary>
    /// Sets the active noise profile by index
    /// </summary>
    /// <param name="profileIndex">Index of the noise profile to use</param>
    public void SetNoiseProfile(int profileIndex)
    {
        if (noiseProfiles != null && profileIndex >= 0 && profileIndex < noiseProfiles.Length)
        {
            activeNoiseProfile = noiseProfiles[profileIndex];
        }
        else if (noiseProfiles != null && noiseProfiles.Length > 0)
        {
            Debug.LogWarning($"Invalid noise profile index: {profileIndex}. Using default.");
            activeNoiseProfile = noiseProfiles[defaultNoiseProfileIndex];
        }
        else
        {
            Debug.LogWarning("No noise profiles configured. Using fallback.");
            activeNoiseProfile = CreateFallbackNoiseProfile();
        }
    }

    private void InitializeBouncing()
    {
        // Set initial position
        currentAnchoredPosition = rectTransform.anchoredPosition;
        groundLevel = currentAnchoredPosition.y;

        // Initialize noise profile
        if (activeNoiseProfile == null)
        {
            if (noiseProfiles != null && noiseProfiles.Length > 0)
            {
                activeNoiseProfile = noiseProfiles[defaultNoiseProfileIndex];
            }
            else
            {
                activeNoiseProfile = CreateFallbackNoiseProfile();
            }
        }

        // Apply noise to initial velocity
        currentVelocity = ApplyNoiseToVector(initialVelocity);
        isInitialized = true;
    }

    private NoiseProfile CreateFallbackNoiseProfile()
    {
        return new NoiseProfile(
            "Fallback",
            new Vector2(0.8f, 1.2f),
            new Vector2(-0.5f, 0.5f),
            10f
        );
    }

    private Vector2 ApplyNoiseToVector(Vector2 baseVector)
    {
        float randomXVelocity = baseVector.x * Random.Range(
            activeNoiseProfile.horizontalDirectionRange.x,
            activeNoiseProfile.horizontalDirectionRange.y
        );

        float randomYVelocity = baseVector.y * Random.Range(
            activeNoiseProfile.velocityRange.x,
            activeNoiseProfile.velocityRange.y
        );

        return new Vector2(randomXVelocity, randomYVelocity);
    }

    private void ApplyBouncingPhysics()
    {
        // Apply gravity
        currentVelocity.y += gravity * Time.deltaTime;

        // Apply position offset noise
        Vector2 positionNoise = new Vector2(
            Random.Range(-activeNoiseProfile.positionOffsetIntensity, activeNoiseProfile.positionOffsetIntensity),
            Random.Range(-activeNoiseProfile.positionOffsetIntensity, activeNoiseProfile.positionOffsetIntensity)
        );

        // Update position
        currentAnchoredPosition += (currentVelocity * Time.deltaTime) + positionNoise;

        // Ground collision and bounce
        if (currentAnchoredPosition.y < groundLevel)
        {
            currentAnchoredPosition.y = groundLevel;
            currentVelocity.y = -currentVelocity.y * bounceDamping;
            currentVelocity.x *= horizontalDrag;

            // Stop bouncing if velocity is too low
            if (Mathf.Abs(currentVelocity.y) < 50f)
            {
                currentVelocity = Vector2.zero;
            }
        }

        rectTransform.anchoredPosition = currentAnchoredPosition;
    }

    private void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float startAlpha = textComponent.color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadingTime)
        {
            float alpha = Mathf.Lerp(startAlpha, 0, elapsedTime / fadingTime);
            textComponent.color = new Color(
                textComponent.color.r,
                textComponent.color.g,
                textComponent.color.b,
                alpha
            );

            Debug.Log("Fading, alpha now at " + alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("finished fading");
        Destroy(gameObject);
    }
}

[System.Serializable]
public class NoiseProfile
{
    public string profileName = "New Profile";
    public Vector2 velocityRange = new Vector2(0.8f, 1.2f);
    public Vector2 horizontalDirectionRange = new Vector2(-0.5f, 0.5f);
    public float positionOffsetIntensity = 10f;

    public NoiseProfile(string name, Vector2 velocityRange, Vector2 horizontalRange, float positionIntensity)
    {
        profileName = name;
        this.velocityRange = velocityRange;
        horizontalDirectionRange = horizontalRange;
        positionOffsetIntensity = positionIntensity;
    }
}
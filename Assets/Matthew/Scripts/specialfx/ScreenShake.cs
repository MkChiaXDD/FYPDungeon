using System.Collections;
using UnityEngine;

public enum ShakeType
{
    Rotational,
    Translational,
    Both
}

/// <summary>
/// Creates a Perlin noise-based camera shake effect with options for rotation and translation
/// </summary>
[DisallowMultipleComponent]
public class ScreenShake : MonoBehaviour
{
    [Header("Shake Type")]
    [Tooltip("Type of shake to apply")]
    [SerializeField] private ShakeType _shakeType = ShakeType.Rotational;

    [Header("Rotational Shake Configuration")]
    [Tooltip("Total duration of the shake effect in seconds")]
    [SerializeField] private float _shakeDuration = 0.5f;

    [Tooltip("Maximum rotation intensity in degrees")]
    [SerializeField] private float _shakeMagnitude = 5f;

    [Tooltip("How quickly the shake settles after completing")]
    [SerializeField] private float _dampingSpeed = 1.5f;

    [Header("Rotational Noise Settings")]
    [Tooltip("Speed of the Perlin noise sampling for rotation")]
    [SerializeField] private float _rotationalNoiseSpeed = 2f;

    [Header("Translational Shake")]
    [Tooltip("Maximum translation intensity in local space")]
    [SerializeField] private Vector3 _translationalShakeMagnitude = new Vector3(0.5f, 0.5f, 0f);

    [Tooltip("Speed of the Perlin noise sampling for translation")]
    [SerializeField] private float _translationalNoiseSpeed = 2f;

    [Tooltip("Use separate noise offset for translational shake")]
    [SerializeField] private bool _useSeparateNoiseForTranslation = true;

    [Tooltip("Enable translational shake on X-axis")]
    [SerializeField] private bool _enableX = true;

    [Tooltip("Enable translational shake on Y-axis")]
    [SerializeField] private bool _enableY = true;

    [Tooltip("Enable translational shake on Z-axis")]
    [SerializeField] private bool _enableZ = true;

    private Quaternion _originalLocalRotation;
    private Vector3 _originalLocalPosition;
    private float _rotationalNoiseOffset;
    private float _translationalNoiseOffset;
    private bool _isShaking;

    private void Awake()
    {
        InitializeNoiseOffsets();
    }

    /// <summary>
    /// Initializes Perlin noise seeds
    /// </summary>
    private void InitializeNoiseOffsets()
    {
        _rotationalNoiseOffset = Random.Range(0f, 100f);
        _translationalNoiseOffset = _useSeparateNoiseForTranslation
            ? Random.Range(0f, 100f)
            : _rotationalNoiseOffset;
    }

    private void Update()
    {
        if (!_isShaking && (NeedsRotationReset() || NeedsPositionReset()))
        {
            SmoothlyResetTransform();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Press F to test Shake() in ScreenShake.cs");
            Shake();
        }
    }

    private bool NeedsRotationReset()
    {
        return _shakeType != ShakeType.Translational &&
               Quaternion.Angle(transform.localRotation, _originalLocalRotation) > 0.01f;
    }

    private bool NeedsPositionReset()
    {
        return _shakeType != ShakeType.Rotational &&
               Vector3.Distance(transform.localPosition, _originalLocalPosition) > 0.01f;
    }

    /// <summary>
    /// Smoothly resets both rotation and position to original values
    /// </summary>
    private void SmoothlyResetTransform()
    {
        if (_shakeType != ShakeType.Translational)
        {
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                _originalLocalRotation,
                Time.deltaTime * _dampingSpeed
            );
        }

        if (_shakeType != ShakeType.Rotational)
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                _originalLocalPosition,
                Time.deltaTime * _dampingSpeed
            );
        }
    }

    /// <summary>
    /// Triggers the camera shake effect
    /// </summary>
    public void Shake()
    {
        StartCoroutine(PerformShakeRoutine());
    }

    private IEnumerator PerformShakeRoutine()
    {
        InitializeShakeState();
        CacheOriginalTransform();

        float elapsedTime = 0f;
        float originalZRotation = _originalLocalRotation.eulerAngles.z;

        while (elapsedTime < _shakeDuration)
        {
            float currentIntensity = CalculateCurrentIntensity(elapsedTime);

            if (_shakeType == ShakeType.Rotational || _shakeType == ShakeType.Both)
            {
                float zRotation = CalculateZRotation(originalZRotation, currentIntensity, elapsedTime);
                ApplyRotation(zRotation);
            }

            if (_shakeType == ShakeType.Translational || _shakeType == ShakeType.Both)
            {
                Vector3 offset = CalculateTranslationalOffset(currentIntensity, elapsedTime);
                ApplyTranslation(offset);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        FinalizeShake();
    }

    /// <summary>
    /// Stores original transform state at shake start
    /// </summary>
    private void CacheOriginalTransform()
    {
        _originalLocalRotation = transform.localRotation;
        _originalLocalPosition = transform.localPosition;
    }

    private void InitializeShakeState()
    {
        _isShaking = true;
    }

    private float CalculateCurrentIntensity(float elapsedTime)
    {
        return 1 - (elapsedTime / _shakeDuration);
    }

    private float CalculateZRotation(float originalZ, float intensity, float elapsedTime)
    {
        float noiseValue = SampleRotationalNoise(elapsedTime);
        return originalZ + (noiseValue * _shakeMagnitude * intensity);
    }

    private float SampleRotationalNoise(float elapsedTime)
    {
        return Mathf.PerlinNoise(_rotationalNoiseOffset, elapsedTime * _rotationalNoiseSpeed) * 2 - 1;
    }

    private void ApplyRotation(float zRotation)
    {
        transform.localRotation = Quaternion.Euler(
            _originalLocalRotation.eulerAngles.x,
            _originalLocalRotation.eulerAngles.y,
            zRotation
        );
    }

    private Vector3 CalculateTranslationalOffset(float intensity, float elapsedTime)
    {
        Vector3 offset = new Vector3(
            _enableX ? SampleTranslationalNoise(elapsedTime, 0) : 0f,
            _enableY ? SampleTranslationalNoise(elapsedTime, 100) : 0f,
            _enableZ ? SampleTranslationalNoise(elapsedTime, 200) : 0f
        );

        offset.x *= _translationalShakeMagnitude.x * intensity;
        offset.y *= _translationalShakeMagnitude.y * intensity;
        offset.z *= _translationalShakeMagnitude.z * intensity;

        return offset;
    }

    private float SampleTranslationalNoise(float elapsedTime, float offsetSeed)
    {
        float noiseOffset = _useSeparateNoiseForTranslation
            ? _translationalNoiseOffset
            : _rotationalNoiseOffset;

        return Mathf.PerlinNoise(noiseOffset + offsetSeed, elapsedTime * _translationalNoiseSpeed) * 2 - 1;
    }

    private void ApplyTranslation(Vector3 offset)
    {
        transform.localPosition = _originalLocalPosition + offset;
    }

    private void FinalizeShake()
    {
        _isShaking = false;
        InitializeNoiseOffsets(); // Refresh noise offsets for next shake
    }
}
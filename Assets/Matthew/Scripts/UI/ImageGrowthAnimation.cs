using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageGrowthAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _growthDuration = 0.5f;
    [SerializeField] private float _shrinkDuration = 0.2f;
    [SerializeField] private float _maxScale = 1.5f;
    [SerializeField] private float _endScale = 1.2f;
    [SerializeField] private AnimationCurve _growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _shrinkCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optional Settings")]
    [SerializeField] private bool _playOnEnable = true;
    [SerializeField] private bool _loopAnimation = false;

    private RectTransform _rectTransform;
    private Vector3 _originalScale;
    private Coroutine _animationCoroutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _originalScale = _rectTransform.localScale;
    }

    private void OnEnable()
    {
        if (_playOnEnable)
        {
            PlayAnimation();
        }
    }

    public void PlayAnimation()
    {
        // Stop any existing animation
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }

        // Reset to original scale
        _rectTransform.localScale = Vector3.zero;

        // Start new animation
        _animationCoroutine = StartCoroutine(GrowAndShrinkAnimation());
    }

    private IEnumerator GrowAndShrinkAnimation()
    {
        do
        {
            // Growth phase
            float growthTimer = 0f;
            while (growthTimer < _growthDuration)
            {
                growthTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(growthTimer / _growthDuration);
                float curveValue = _growthCurve.Evaluate(progress);
                _rectTransform.localScale = Vector3.Lerp(Vector3.zero, _originalScale * _maxScale, curveValue);
                yield return null;
            }

            // Shrink phase
            float shrinkTimer = 0f;
            while (shrinkTimer < _shrinkDuration)
            {
                shrinkTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(shrinkTimer / _shrinkDuration);
                float curveValue = _shrinkCurve.Evaluate(progress);
                _rectTransform.localScale = Vector3.Lerp(_originalScale * _maxScale, _originalScale * _endScale, curveValue);
                yield return null;
            }

            // Ensure final scale is exact
            _rectTransform.localScale = _originalScale * _endScale;

            if (_loopAnimation)
            {
                // Reset before looping
                _rectTransform.localScale = Vector3.zero;
            }

        } while (_loopAnimation);
    }

    private void OnDisable()
    {
        // Reset scale when disabled
        if (_rectTransform != null)
        {
            _rectTransform.localScale = _originalScale;
        }

        // Stop any running coroutine
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }
}
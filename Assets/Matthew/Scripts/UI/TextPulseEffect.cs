using System.Collections;
using TMPro;
using UnityEngine;

public class TextPulseEffect : MonoBehaviour
{   
    //Component
    [SerializeField] private TMP_Text _targetText;

    [Header("Text settings")]
    [SerializeField] private float _pulseDuration = 1f;
    [SerializeField] private float _sizeMultiplier = 1.5f;

    private float _originalSize;
    private Coroutine _pulseCoroutine;

    private void Awake()
    {
        InitializeTextSize();
    }

    /// <summary>
    /// Initialises the original TextSize 
    /// keep tracks of the font size
    /// </summary>
    void InitializeTextSize()
    {
        _originalSize = _targetText.fontSize;
    }

    /// <summary>
    /// Helper function to Start the pulseanimationcoroutine
    /// </summary>
    public void StartPulseAnimation()
    {
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
        }
        _pulseCoroutine = StartCoroutine(PulseAnimation());
    }

    /// <summary>
    /// Pulse animation for text
    /// Played when the numbers (no of punches etc) for score calculations come out
    /// </summary>
    private IEnumerator PulseAnimation()
    {
        // Grow phase
        yield return ChangeTextSize(_originalSize * _sizeMultiplier, _pulseDuration / 2);

        // Shrink phase
        yield return ChangeTextSize(_originalSize, _pulseDuration / 2);
    }

    /// <summary>
    /// Changes the size of the Text
    /// </summary>
    private IEnumerator ChangeTextSize(float targetSize, float duration)
    {
        float initialSize = _targetText.fontSize;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            //use unscaledDeltaTime since gamescene is paused after game ends
            elapsedTime += Time.unscaledDeltaTime;
            _targetText.fontSize = Mathf.Lerp(initialSize, targetSize, elapsedTime / duration);
            yield return null;
        }

        // Ensure final size is exact since lerping sometimes doesnt reach the end
        _targetText.fontSize = targetSize;
    }
}
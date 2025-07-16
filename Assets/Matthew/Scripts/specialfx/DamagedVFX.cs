using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagedVFX : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private int flashCount = 2;
    [SerializeField] private bool affectChildren = true;

    private List<SkinnedMeshRenderer> _renderers = new List<SkinnedMeshRenderer>();
    private Dictionary<SkinnedMeshRenderer, Color> _originalColors = new Dictionary<SkinnedMeshRenderer, Color>();
    private Coroutine _flashRoutine;
    private MaterialPropertyBlock _propBlock;

    private void Awake()
    {
        // Initialize material property block
        _propBlock = new MaterialPropertyBlock();

        // Find all relevant renderers
        FindRenderers();
    }

    private void FindRenderers()
    {
        _renderers.Clear();
        _originalColors.Clear();

        // Get renderers based on selection
        if (affectChildren)
        {
            // Get all renderers in children (including SkinnedMeshRenderers)
            SkinnedMeshRenderer[] childRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            _renderers.AddRange(childRenderers);
        }
        else
        {
            // Get only renderers on this specific GameObject
            SkinnedMeshRenderer selfRenderer = GetComponent<SkinnedMeshRenderer>();
            if (selfRenderer != null)
            {
                _renderers.Add(selfRenderer);
            }
        }

        // Store original colors for each renderer
        foreach (SkinnedMeshRenderer renderer in _renderers)
        {
            renderer.GetPropertyBlock(_propBlock);
            _originalColors[renderer] = _propBlock.GetColor("Color");

            // Fallback to _Color if _BaseColor not found (for standard shaders)
            if (!_propBlock.HasProperty("Color"))
            {
                _originalColors[renderer] = renderer.material.GetColor("Color");
            }
        }
    }

    public void TriggerDamageFlash()
    {
        // Stop existing flash if one is running
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }

        _flashRoutine = StartCoroutine(FlashEffect());
    }

    private IEnumerator FlashEffect()
    {
        float elapsedTime = 0f;
        float flashInterval = flashDuration / flashCount;
        int currentFlash = 0;

        while (currentFlash < flashCount)
        {
            // Flash to damage color
            elapsedTime = 0f;
            while (elapsedTime < flashInterval / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (flashInterval / 2);
                SetAllRenderersColor(Color.Lerp(_originalColors[_renderers[0]], damageColor, t));
                yield return null;
            }

            // Flash back to original color
            elapsedTime = 0f;
            while (elapsedTime < flashInterval / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (flashInterval / 2);
                SetAllRenderersColor(Color.Lerp(damageColor, _originalColors[_renderers[0]], t));
                yield return null;
            }

            currentFlash++;
        }

        // Ensure we return to original color
        ResetAllRenderersToOriginalColor();
        _flashRoutine = null;
    }

    private void SetAllRenderersColor(Color color)
    {
        foreach (SkinnedMeshRenderer renderer in _renderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("Color", color);
            _propBlock.SetColor("Color", color); // Set both common color properties
            renderer.SetPropertyBlock(_propBlock);
        }
    }

    private void ResetAllRenderersToOriginalColor()
    {
        foreach (SkinnedMeshRenderer renderer in _renderers)
        {
            if (renderer == null) continue;

            renderer.GetPropertyBlock(_propBlock);

            if (_originalColors.TryGetValue(renderer, out Color originalColor))
            {
                _propBlock.SetColor("Color", originalColor);
                _propBlock.SetColor("Color", originalColor);
            }
            else
            {
                // Fallback to white if original color not found
                _propBlock.SetColor("Color", Color.white);
                _propBlock.SetColor("Color", Color.white);
            }

            renderer.SetPropertyBlock(_propBlock);
        }
    }

    private void OnDisable()
    {
        // Reset color when disabled
        ResetAllRenderersToOriginalColor();
    }

    // Editor function to preview flash effect
    [ContextMenu("Test Damage Flash")]
    public void TestDamageFlash()
    {
        TriggerDamageFlash();
    }
}
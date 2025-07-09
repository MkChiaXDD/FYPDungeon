using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ElementalStatus : MonoBehaviour
{
    // Tracks active elements and their gauges
    private Dictionary<ElementType, float> _elementGauges = new();

    [Header("Debug Settings")]
    [SerializeField] private bool _logGaugeChanges = true;

    // Apply element to target
    public void ApplyElement(ElementType element, float gaugeUnits)
    {

       

        // Initialize if needed
        if (!_elementGauges.ContainsKey(element))
        {
            DebugLogGauge($"Initializing {element} gauge", element);
            _elementGauges[element] = gaugeUnits;

           

        }

        Debug.LogWarning(gaugeUnits);



        float oldValue = _elementGauges[element];
        _elementGauges[element] = Mathf.Clamp(_elementGauges[element] + gaugeUnits, 0, 2f);

       

        DebugLogGauge($"{element} gauge changed: {oldValue} → {_elementGauges[element]} " +
                     $"(Δ: {gaugeUnits})", element);

        // Immediately remove if gauge depletes
        if (_elementGauges[element] <= 0.1f)
        {
            DebugLogGauge($"Removing depleted {element} gauge", element);
            _elementGauges.Remove(element);
        }
    }

    // Get all active elements
    public Dictionary<ElementType, float> GetActiveElements() =>
        new Dictionary<ElementType, float>(_elementGauges);

    // Element decay over time
    void Update()
    {
        foreach (var element in _elementGauges.Keys.ToList())
        {
            float oldValue = _elementGauges[element];
            _elementGauges[element] -= Time.deltaTime * 0.3f; // Decay rate

            DebugLogGauge($"{element} decay: {oldValue} → {_elementGauges[element]} " +
                         $"(Δ: {Time.deltaTime * -0.3f})", element);

            // Remove when gauge depletes
            if (_elementGauges[element] <= 0.01f)
            {
                DebugLogGauge($"Removing decayed {element} gauge", element);
                _elementGauges.Remove(element);
            }
        }
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        int i = 0;
        foreach (var element in _elementGauges)
        {
            Color color = GetElementColor(element.Key);
            Gizmos.color = color;
            Vector3 position = transform.position + Vector3.up * (1 + i * 0.3f);
            Gizmos.DrawWireSphere(position, element.Value * 0.2f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(position, $"{element.Key}: {element.Value:F1}");
#endif

            i++;
        }
    }

    private Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Pyro => Color.red,
            ElementType.Hydro => Color.blue,
            ElementType.Electro => Color.yellow,
            ElementType.Cryo => Color.cyan,
            _ => Color.white
        };
    }

    private void DebugLogGauge(string message, ElementType element)
    {
        if (!_logGaugeChanges) return;

        string fullMessage = $"[{name}] {message}";
        Debug.Log(fullMessage);

        // Add color-coded console messages
        string colorTag = ColorUtility.ToHtmlStringRGB(GetElementColor(element));
        //Debug.Log($"<color=#{colorTag}>{fullMessage}</color>");
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElementalStatus : MonoBehaviour
{
    // Tracks active elements and their gauges
    private Dictionary<ElementType, float> _elementGauges = new();

    // Apply element to target
    public void ApplyElement(ElementType element, float duration)
    {
        if (!_elementGauges.ContainsKey(element))
            _elementGauges[element] = 0f;

        _elementGauges[element] = Mathf.Clamp(_elementGauges[element] + duration, 0, 2f);
    }

    // Get all active elements
    public Dictionary<ElementType, float> GetActiveElements() =>
        new Dictionary<ElementType, float>(_elementGauges);

    // Element decay over time
    void Update()
    {
        foreach (var element in _elementGauges.Keys.ToList())
        {
            _elementGauges[element] -= Time.deltaTime * 0.3f; // Decay rate
            if (_elementGauges[element] <= 0.01f)
                _elementGauges.Remove(element);
        }
    }

    //Add debug view
    void OnDrawGizmos()
    {
        foreach (var element in _elementGauges)
        {
            Color color = GetElementColor(element.Key);
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, element.Value);
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
}


using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    //[SerializeField] private float floatSpeed = 2f;
    //[SerializeField] private float lifeTime = 1f;
    //[SerializeField] private float scaleAmount = 0.5f;

    //private TextMeshPro textMesh;
    //private Vector3 startPosition;
    //private float startTime;

    //// Element color mapping
    //private static readonly Color PyroColor = new Color(1f, 0.4f, 0f);
    //private static readonly Color HydroColor = new Color(0.2f, 0.5f, 1f);
    //private static readonly Color ElectroColor = new Color(1f, 0.9f, 0.2f);
    //private static readonly Color CryoColor = new Color(0.4f, 0.8f, 1f);

    //void Awake()
    //{
    //    textMesh = GetComponent<TextMeshPro>();
    //    startPosition = transform.position;
    //    startTime = Time.time;
    //}

    //public void SetDamage(float amount, ElementType element)
    //{
    //    textMesh.text = Mathf.RoundToInt(amount).ToString();
    //    textMesh.color = GetElementColor(element);

    //    // Scale based on damage amount
    //    float scale = 1f + Mathf.Clamp(amount / 50f, 0f, 2f) * scaleAmount;
    //    transform.localScale = Vector3.one * scale;
    //}

    //void Update()
    //{
    //    // Float upward
    //    transform.position = startPosition + Vector3.up * (Time.time - startTime) * floatSpeed;

    //    // Fade out
    //    float t = (Time.time - startTime) / lifeTime;
    //    textMesh.alpha = 1f - t;

    //    // Scale down
    //    transform.localScale = Vector3.one * (1f - t * 0.5f);

    //    // Destroy after lifetime
    //    if (Time.time > startTime + lifeTime)
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    //private Color GetElementColor(ElementType element)
    //{
    //    return element switch
    //    {
    //        ElementType.Pyro => PyroColor,
    //        ElementType.Hydro => HydroColor,
    //        ElementType.Electro => ElectroColor,
    //        ElementType.Cryo => CryoColor,
    //        _ => Color.white
    //    };
    //}
}
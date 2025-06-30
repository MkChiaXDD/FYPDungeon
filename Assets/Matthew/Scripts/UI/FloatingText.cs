using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    //[SerializeField] private float floatSpeed = 1f;
    //[SerializeField] private float lifeTime = 1.5f;

    //[SerializeField] private TMP_Text floatingTextPrefab;

    //private TextMeshPro textMesh;
    //private float startTime;
    //private Vector3 startPosition;
    

    //public static FloatingText Create(Vector3 position, string text, Color color)
    //{
    //    GameObject prefab = floatingTextPrefab;
    //    GameObject instance = Instantiate(prefab, position, Quaternion.identity);
    //    FloatingText floatingText = instance.GetComponent<FloatingText>();
    //    floatingText.Initialize(text, color);
    //    return floatingText;
     
    //}

    //private void Initialize(string text, Color color)
    //{
    //    textMesh = GetComponent<TextMeshPro>();
    //    textMesh.text = text;
    //    textMesh.color = color;
    //    startPosition = transform.position;
    //    startTime = Time.time;
    //}

    //void Update()
    //{
    //    // Float upward
    //    transform.position = startPosition + Vector3.up * (Time.time - startTime) * floatSpeed;

    //    // Fade out
    //    float t = (Time.time - startTime) / lifeTime;
    //    textMesh.alpha = 1f - t;

    //    // Destroy after lifetime
    //    if (Time.time > startTime + lifeTime)
    //    {
    //        Destroy(gameObject);
    //    }
    //}
}
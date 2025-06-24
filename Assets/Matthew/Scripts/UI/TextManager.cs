using TMPro;
using UnityEngine;

/// <summary>
/// TextManager class handles dynamically-created Text
/// </summary>
public class TextManager : MonoBehaviour
{
    //singleton
    public static TextManager TextInstance;

    //Components needed
    [SerializeField]
    private GameObject textPrefab;
    [SerializeField]
    private RectTransform canvasTransform;

    public bool disableText;

    [SerializeField]
    private float randomisedRange = 0.1f;

    private void Awake()
    {
        InitialiseSingleton();
    }

    /// <summary>
    /// Creates a text from a prefab
    /// Call for temporary UI related purposes, fading text. If not, manually create text,
    /// </summary>
    public void CreateText(Vector3 position, string text, Color colour)
    {
        if (disableText == true)
            return;
        GameObject _Text = Instantiate(textPrefab, RandomiseOffsetPosition(position), Quaternion.identity);
        _Text.transform.SetParent(canvasTransform);
        _Text.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 2);
        _Text.GetComponent<TMP_Text>().text = text;
        _Text.GetComponent<TMP_Text>().color = colour;
    }

    private Vector3 RandomiseOffsetPosition(Vector3 position)
    {
        Vector3 randomisedOffset = new Vector3(position.x * Random.Range(1 - randomisedRange, 1 + randomisedRange), position.y , position.z) ;
        return randomisedOffset;
    }


    /// <summary>
    /// Create a singleton to have dynamically-created text in scripts needed
    /// </summary>
    private void InitialiseSingleton()
    {
        if (!TextInstance)
        {
            TextInstance = this;
        }
    }

    public void DisableCreating()
    {
        disableText = true;
    }

    public void EnableCreating()
    {
        disableText = false;
    }
}



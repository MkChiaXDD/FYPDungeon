using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance;

    [Header("References")]
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private RectTransform canvasTransform;

    [Header("Text Settings")]
    [SerializeField] private bool disableText;
    [SerializeField] private float positionRandomRange = 25f;
    [SerializeField] private int defaultNoiseProfileIndex;

    private Canvas canvasComponent;

    private void Awake()
    {
        InitializeSingleton();
        canvasComponent = canvasTransform.GetComponent<Canvas>();
    }

    public void CreateText(Vector3 worldPosition, string text, Color color, int noiseProfileIndex = -1)
    {
        if (disableText) return;

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            screenPosition,
            canvasComponent.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvasComponent.worldCamera,
            out anchoredPosition
        );

        // Apply random position offset
        anchoredPosition += Random.insideUnitCircle * positionRandomRange;

        GameObject textObj = Instantiate(textPrefab, canvasTransform);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchoredPosition = anchoredPosition;
        textRect.localScale = Vector3.one * 2;

        TMP_Text tmpComponent = textObj.GetComponent<TMP_Text>();
        tmpComponent.text = text;
        tmpComponent.color = color;

        // Set noise profile if specified
        FadingText fadingText = textObj.GetComponent<FadingText>();
        if (noiseProfileIndex >= 0)
        {
            fadingText.SetNoiseProfile(noiseProfileIndex);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            CreateText(new Vector3(1, 1, 1), "gay nigga", Color.black);
        }
    }
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DisableText() => disableText = true;
    public void EnableText() => disableText = false;
}
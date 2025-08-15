using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public static CustomCursor Instance { get; private set; }

    [Header("Default Cursor")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Vector2 defaultHotspot = Vector2.zero;

    [Header("Click Cursor")]
    [SerializeField] private Texture2D clickCursor;
    [SerializeField] private Vector2 clickHotspot = Vector2.zero;

    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private bool usingClickCursor = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetCursor(defaultCursor, defaultHotspot);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (!usingClickCursor)
            {
                SetCursor(clickCursor, clickHotspot);
                usingClickCursor = true;
            }
        }
        else
        {
            if (usingClickCursor)
            {
                SetCursor(defaultCursor, defaultHotspot);
                usingClickCursor = false;
            }
        }
    }

    public void SetCursor(Texture2D texture, Vector2 hotspot)
    {
        Cursor.SetCursor(texture, hotspot, cursorMode);
    }
}

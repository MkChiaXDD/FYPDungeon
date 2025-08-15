using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Canvas worldCanvas;


    [Header("Debug Mode")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private KeyCode debugToggleKey = KeyCode.F3;
    [SerializeField] private KeyCode debugTestKey = KeyCode.F4;
    [SerializeField] private Color debugColor = Color.magenta;
    [SerializeField] private bool logDebugEvents = true;
    [SerializeField] private string debugDamageText = "01";
    [SerializeField] private Vector3 debugTestOffset = new Vector3(0, 2f, 0);

    public static DamageNumberManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;            
        }
        InitializeCanvas();
    }

    private void InitializeCanvas()
    {
        if (worldCanvas == null)
        {
            worldCanvas = FindObjectOfType<Canvas>();
            if (worldCanvas == null)
            {
                GameObject canvasGO = new GameObject("WorldCanvas");
                worldCanvas = canvasGO.AddComponent<Canvas>();
                worldCanvas.renderMode = RenderMode.WorldSpace;

                // REMINDEr these settings
                worldCanvas.transform.localScale = Vector3.one * 0.01f; // Try increasing if too small
                worldCanvas.sortingOrder = 100; // Ensure it renders on top
                worldCanvas.worldCamera = Camera.main; // Explicitly assign camera
            }
        }
    }

    public void ShowDamage(Vector3 position, float damage, ElementType element)
    {
        if (damageNumberPrefab == null) return;

        // Position slightly above the hit point
        Vector3 spawnPosition = position + Vector3.up * 2.0f;

        GameObject numberGO = Instantiate(
            damageNumberPrefab,
            spawnPosition,
            Quaternion.identity,
            worldCanvas.transform
        );

        DamageNumber damageNumber = numberGO.GetComponentInChildren<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.Initialize(damage, element);
        }
    }

    public void ShowDamage(Vector3 position, float damage, PhysicalAttackType attackType)
    {
        if (damageNumberPrefab == null) return;

        // Position slightly above the hit point
        Vector3 spawnPosition = position + Vector3.up * 2.0f;

        GameObject numberGO = Instantiate(
            damageNumberPrefab,
            spawnPosition,
            Quaternion.identity
        );

        DamageNumber damageNumber = numberGO.GetComponentInChildren<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.Initialize(damage, attackType);
        }
    }



    public void ShowDamage(Vector3 position, float damage)
    {
        if (damageNumberPrefab == null) return;

        // Position slightly above the hit point
        Vector3 spawnPosition = position + Vector3.up * 1.0f;

        GameObject numberGO = Instantiate(
            damageNumberPrefab,
            spawnPosition,
            Quaternion.identity
        );

        DamageNumber damageNumber = numberGO.GetComponentInChildren<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.Initialize(damage);
        }
    }

    public void ShowDamage(Vector3 position, float damage, Color color)
    {
        if (damageNumberPrefab == null) return;

        // Position slightly above the hit point
        Vector3 spawnPosition = position + Vector3.up * 1.0f;

        GameObject numberGO = Instantiate(
            damageNumberPrefab,
            spawnPosition,
            Quaternion.identity
        );

        DamageNumber damageNumber = numberGO.GetComponentInChildren<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.Initialize(damage,color);
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ShowDebugDamageNumber();
        }
    }

    private void ShowDebugDamageNumber()
    {
        Vector3 testPosition = transform.position + debugTestOffset;
        GameObject numberGO = Instantiate(
            damageNumberPrefab,
            testPosition,
            Quaternion.identity
        );

        DamageNumber damageNumber = numberGO.GetComponentInChildren<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.Initialize(debugDamageText, debugColor);

            if (logDebugEvents)
            {
                Debug.Log($"[DEBUG] Showing test damage number: {debugDamageText}");
            }
        }
    }
}
using UnityEngine;

public class PopUpManager : MonoBehaviour
{
    public static PopUpManager Instance { get; private set; }

    [SerializeField] private GameObject popUpPrefab;
    [SerializeField] private Transform canvasTransform;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public static void ShowPopUp(string message, float duration, Color color)
    {
        if (Instance == null) return;

        GameObject popup = Instantiate(Instance.popUpPrefab, Instance.canvasTransform);
        popup.GetComponent<PopUpText>().Initialize(message, duration, color);
    }
}

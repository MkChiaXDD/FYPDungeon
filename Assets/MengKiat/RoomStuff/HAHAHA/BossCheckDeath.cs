using UnityEngine;
using UnityEngine.Events;

public class BossCheckDeath : MonoBehaviour
{
    public UnityEvent OnLevelEnd;
    public BuffSelectionUI BuffSelectionUI;

    public void OnEnable()
    {
        FindAnyObjectByType<BuffSelectionUI>().Spawn.AddListener(SetBuffUI);
        SetBuffUI();

    }

    public void DieProceed()
    {
        //OnLevelEnd?.Invoke();
        BuffSelectionUI.Select();
        BuffSelectionUI.CreateBuffCardUI();
        FindFirstObjectByType<FarthestRoom>().NextLevel();
    }

    public void SetBuffUI()
    {
        BuffSelectionUI = FindAnyObjectByType<BuffSelectionUI>();
    }
}

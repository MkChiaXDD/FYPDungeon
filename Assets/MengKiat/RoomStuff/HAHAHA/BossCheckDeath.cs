using UnityEngine;
using UnityEngine.Events;

public class BossCheckDeath : MonoBehaviour
{
    public UnityEvent OnLevelEnd;
    public BuffSelectionUI BuffSelectionUI;
    private GameObject portal;

    public void OnEnable()
    {
        FindAnyObjectByType<BuffSelectionUI>().Spawn.AddListener(SetBuffUI);
        SetBuffUI();

    }

    public void SummonPortal()
    {
        Debug.Log("Finding Portal");
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "PORTAL(Clone)")
            {
                portal = obj;
                portal.SetActive(true);
                return;
            }
        }
        Debug.LogError("Can't find portal");

    }

    public void SetBuffUI()
    {
        BuffSelectionUI = FindAnyObjectByType<BuffSelectionUI>();
    }
}

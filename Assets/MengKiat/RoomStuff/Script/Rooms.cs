using UnityEngine;

public class Rooms : MonoBehaviour
{
    [Header("Exit Points")]
    public Transform Exit_Forward;
    public Transform Exit_Back;
    public Transform Exit_Left;
    public Transform Exit_Right;

    [Header("Wall GameObjects")]
    public GameObject Wall_Forward;
    public GameObject Wall_Back;
    public GameObject Wall_Left;
    public GameObject Wall_Right;

    [HideInInspector]
    public Transform[] exitPoints;

    private void Awake()
    {
        // Collect exit points
        exitPoints = GetAllNonNullExits();

        // Activate/deactivate walls based on exits
        UpdateWalls();
    }

    private Transform[] GetAllNonNullExits()
    {
        var exits = new System.Collections.Generic.List<Transform>();
        if (Exit_Forward) exits.Add(Exit_Forward);
        if (Exit_Back) exits.Add(Exit_Back);
        if (Exit_Left) exits.Add(Exit_Left);
        if (Exit_Right) exits.Add(Exit_Right);
        return exits.ToArray();
    }

    private void UpdateWalls()
    {
        if (Wall_Forward) Wall_Forward.SetActive(Exit_Forward == null);
        if (Wall_Back) Wall_Back.SetActive(Exit_Back == null);
        if (Wall_Left) Wall_Left.SetActive(Exit_Left == null);
        if (Wall_Right) Wall_Right.SetActive(Exit_Right == null);
    }
}

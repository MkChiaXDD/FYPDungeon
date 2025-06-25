using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [SerializeField] private AnimationCurve difficultyCurve;

    [SerializeField]
    private List<Vector2> curvePoints = new List<Vector2>();

    private void Awake()
    {
        GenerateDifficultyCurve();
    }

    private void GenerateDifficultyCurve()
    {
        difficultyCurve = new AnimationCurve();

        foreach (Vector2 point in curvePoints)
        {
            difficultyCurve.AddKey(new Keyframe(point.x, point.y));
        }

        difficultyCurve.postWrapMode = WrapMode.ClampForever;
        difficultyCurve.preWrapMode = WrapMode.ClampForever;
    }

    // Example: called by Enemy scripts
    public float GetDifficultyMultiplier()
    {
        return difficultyCurve.Evaluate(currentRound); // assuming you have a currentRound field
    }

    public int GetRound()
    {
        return currentRound;
    }

    [SerializeField]
    private int currentRound = 1;

    public void IncreaseRound()
    {
        currentRound++;
    }
}

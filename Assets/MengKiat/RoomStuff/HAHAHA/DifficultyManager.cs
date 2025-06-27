using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    [SerializeField] private AnimationCurve difficultyCurve;
    [SerializeField] private List<Vector2> curvePoints = new List<Vector2>();
    [SerializeField] private int currentRound = 1;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keep across scenes

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

    public float GetDifficultyMultiplier()
    {
        return difficultyCurve.Evaluate(currentRound);
    }

    public void IncreaseRound()
    {
        currentRound++;
    }

    public int GetRound()
    {
        return currentRound;
    }
}
